using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class Compiler {

    public delegate Dictionary ConnectionGetter(StringName name, int port, bool from);
    public delegate GraphNode NodeGetter(StringName name);

    private NodeGetter nodeGetter; 
    private ConnectionGetter connectionGetter;

    private QCircuit c;

    private Godot.Collections.Dictionary<Dictionary, int> classicalMapping = new();
    private int currentClassicBitIdx = -1;

    public Compiler(NodeGetter nodeGetter, ConnectionGetter connectionGetter) {
        this.nodeGetter = nodeGetter;
        this.connectionGetter = connectionGetter;
        Reset();
    }

    public void Reset() {
        c = null;
        classicalMapping.Clear();
        currentClassicBitIdx = -1;
    }

    public QCircuit compile(int QBitCount, InputGate nodeIn, bool QBitOrderSwapped) {
        Reset();
        c = new QCircuit();
        GraphNode checkpoint = nodeIn;
        var runningSlots = new int[QBitCount];
        for(int i = 0; i < QBitCount; ++i) {
            runningSlots[i] = i;
        } 
        while(!(checkpoint is OutputGate)) {
            c.parts.Add(CompileFromCheckpointToNext(QBitCount, ref checkpoint, ref runningSlots, QBitOrderSwapped));
        }
        foreach(var connection in classicalMapping.Keys) {
            var node = nodeGetter(connection["from_node"].AsStringName());
            if(node != null && node is MeasurementGate gate) {
                c.measurementGatesToBits.Add(gate, classicalMapping[connection]);
            }
        }
        c.bits = new int[c.measurementGatesToBits.Keys.Count];
        return c;
    }

    public QCircuitPart CompileFromCheckpointToNext(int QBitCount, ref GraphNode checkpoint, ref int[] runningSlots, bool QBitOrderSwapped) {
        QCircuitPart part = new(QBitCount);
        var runningNodes = new GraphNode[QBitCount];
        for(int i = 0; i < QBitCount; ++i) {
            runningNodes[i] = checkpoint;
            runningSlots[i] = i;
        } 
        while(!AllAtCheckpointExcluding(runningNodes, checkpoint)){
            var progress = false;
            for(int i = 0; i < QBitCount; ++i) {
                if(runningNodes[i] is ICheckpointGate && runningNodes[i] != checkpoint) continue;
                if(runningNodes[i] is IMultiInputGate mgate) {
                    if(!AllInputsAtGate(runningNodes, runningNodes[i])) continue;
                    var inputs = InputsAtGate(runningNodes, runningNodes[i]);
                    if(runningNodes[i] is ICompileableGate gate) {
                        Array<int> bits = new();
                        for(int j = 0; j < mgate.GetSlotCount(); ++j) {
                            for(int k = 0; k < inputs.Count; ++k) {
                                if(runningSlots[inputs[k]] == j) {
                                    bits.Add(ActualIndex(inputs[k], QBitCount, QBitOrderSwapped));
                                    break;
                                }
                            } 
                            if(bits.Count != j + 1) {
                                Helpers.ShowErrorOnPort(runningNodes[i], j, true);
                                throw new Exception("This circuit is not compilable: Error parsing a Gate with multiple inputs.");
                            }
                        }
                        part.compiledMatrix = gate.compile(QBitCount, bits) * part.compiledMatrix;
                    }
                    foreach(int idx in inputs) {
                        var connection = connectionGetter(runningNodes[idx].Name, runningSlots[idx], true);
                        if(connection == null) {
                            Helpers.ShowErrorOnPort(runningNodes[i], runningSlots[idx], false);
                            throw new Exception("This circuit is not compilable: Some QBits don't reach the output.");
                        }
                        var nextNode = nodeGetter((StringName)connection["to_node"]);
                        runningNodes[idx] = nextNode;
                        runningSlots[idx] = (int)connection["to_port"];
                    }
                    progress = true;
                }
                else {
                    var connection = connectionGetter(runningNodes[i].Name, runningSlots[i], true);
                    if(connection == null) {
                        Helpers.ShowErrorOnPort(runningNodes[i], runningSlots[i], false);
                        throw new Exception("This circuit is not compilable: Some QBits don't reach the output.");
                    } 
                    var nextNode = nodeGetter((StringName)connection["to_node"]);
                    if(nextNode is MeasurementGate mGate) {
                        if(runningNodes[i] != checkpoint) {
                            Helpers.ShowErrorOnNode(nextNode);
                            throw new Exception("This circuit is not compilable: Measurements can only occur directly after a checkpoint.");
                        } 
                        var classicConnection = connectionGetter(nextNode.Name, 1, true);
                        if(classicConnection != null) {
                            classicalMapping.Add(classicConnection, ++currentClassicBitIdx);
                        }
                        part.measurements.Add(i);
                        part.measurementGates.Add(mGate);
                    }
                    if(runningNodes[i] is ICompileableGate gate) {
                        part.compiledMatrix = gate.compile(QBitCount, new Array<int> {ActualIndex(i, QBitCount, QBitOrderSwapped)}) * part.compiledMatrix;
                    }
                    if(runningNodes[i] is ICompileableGateWithBits gateWithBits) {
                        var bits = new Array<int>();
                        for(var x = 0; x < runningNodes[i].GetInputPortCount(); ++x) {
                            if(runningNodes[i].GetInputPortType(x) == 1) {
                                var conn = connectionGetter(runningNodes[i].Name, x, false);
                                if(classicalMapping.ContainsKey(conn)) {
                                    bits.Add(classicalMapping[conn]);
                                    if(runningNodes[i].GetOutputPortCount() > x) {
                                        var conn2 = connectionGetter(runningNodes[i].Name, x, true);
                                        if(conn2 != null) {
                                            classicalMapping.Add(conn2, classicalMapping[conn]);
                                        }
                                    }
                                }else {
                                    Helpers.ShowErrorOnPort(runningNodes[i], x, true);
                                    throw new Exception("This circuit is not compilable: Some gates require a classical bit as input but no measured bit reaches them.");
                                }
                            }
                        }
                        part.compiledMatrix = gateWithBits.compile(QBitCount, new Array<int> {ActualIndex(i, QBitCount, QBitOrderSwapped)}, bits, c.GetMatrixIndexByBits) * part.compiledMatrix;
                    }
                    runningNodes[i] = nextNode;
                    runningSlots[i] = (int)connection["to_port"];
                    progress = true;
                }
            }
            if(!progress) throw new Exception("This circuit is not compilable: Compiler ran into an endless loop.");
        }
        if(!AllAtSameCheckpoint(runningNodes)) {
            var checkpoints = new List<GraphNode>();
            foreach(var n in runningNodes) {
                if(!checkpoints.Contains(n)) checkpoints.Add(n);
            }
            foreach(var n in checkpoints) {
                Helpers.ShowErrorOnNode(n);
            }
            throw new Exception("This circuit is not compilable: All Qbits need to run through every checkpoint and to the output.");
        }
        checkpoint = runningNodes[0];
        return part;
    }

    private bool AllAtEnd(GraphNode[] nodes) {
        foreach (GraphNode node in nodes) {
            if (!(node is OutputGate)) return false;
        }
        return true;
    }

    private bool AllAtSameCheckpoint(GraphNode[] nodes) {
        GraphNode checkpoint = null;
        foreach (GraphNode node in nodes) {
            if (!(node is ICheckpointGate)) return false;
            if (checkpoint == null) checkpoint = node;
            else if (checkpoint != node) return false;
        }
        return true;
    }

    private bool AllAtCheckpointExcluding(GraphNode[] nodes, GraphNode exclude = null) {
        foreach (GraphNode node in nodes) {
            if (!(node is ICheckpointGate)) return false;
            if (exclude != null && node == exclude) return false;
        }
        return true;
    }

    private bool AllInputsAtGate(GraphNode[] nodes, GraphNode gate) {
        if (!(gate is IMultiInputGate mgate)) return false;
        return mgate.GetSlotCount() == InputsAtGate(nodes, gate).Count;
    }

    private Array<int> InputsAtGate(GraphNode[] nodes, GraphNode gate) {
        Array<int> _out = new();
        for(int i = 0; i < nodes.Length; ++i) {
            if(gate == nodes[i])_out.Add(i);
        }
        return _out;
    }

    private int ActualIndex(int i, int QBitCount, bool QBitOrderSwapped) {
        return QBitOrderSwapped ? i : QBitCount - i - 1;
    }
}