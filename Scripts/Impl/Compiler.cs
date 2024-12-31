using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class Compiler {

    public delegate Dictionary ConnectionGetter(StringName name, int port);
    public delegate GraphNode NodeGetter(StringName name);
    public static QCircuit compile(int QBitCount, InputGate nodeIn, NodeGetter nodeGetter, ConnectionGetter connectionGetter) {
        Vector input = new(1 << QBitCount);
        
        //TODO compile input

        var circuit = new QCircuit(input);
        var currentPart = new QCircuitPart(QBitCount);
        var runningNodes = new GraphNode[QBitCount];
        var runningSlots = new int[QBitCount];
        for(int i = 0; i < QBitCount; ++i) {
            runningNodes[i] = nodeIn;
            runningSlots[i] = i;
        } 
        GraphNode lastCheckpoint = null;
        while(!AllAtEnd(runningNodes)) {
            while(!AllAtCheckpointExcluding(runningNodes, lastCheckpoint)){
                var progress = false;
                for(int i = 0; i < QBitCount; ++i) {
                    if(runningNodes[i] is ICheckpointGate && runningNodes[i] != lastCheckpoint) continue;
                    if(runningNodes[i] is IMultiInputGate mgate) {
                        if(!AllInputsAtGate(runningNodes, runningNodes[i])) continue;
                        var inputs = InputsAtGate(runningNodes, runningNodes[i]);
                        if(runningNodes[i] is ICompileableGate gate) {
                            Array<int> bits = new();
                            for(int j = 0; j < mgate.GetSlotCount(); ++j) {
                                for(int k = 0; k < inputs.Count; ++k) {
                                    if(runningSlots[inputs[k]] == j) {
                                        bits.Add(k);
                                        break;
                                    }
                                } 
                                if(bits.Count != j) {
                                    throw new Exception("This circuit is not compilable: Error parsing a Gate with multiple inputs.");
                                }
                            }
                            gate.compile(QBitCount, bits);
                        }
                        foreach(int idx in inputs) {
                            var connection = connectionGetter(runningNodes[idx].Name, runningSlots[idx]);
                            if(connection == null) throw new Exception("This circuit is not compilable: Some QBits don't reach the output.");
                            var nextNode = nodeGetter((StringName)connection["to_node"]);
                            runningNodes[idx] = nextNode;
                            runningSlots[idx] = (int)connection["to_port"];
                        }
                        progress = true;
                    }
                    else {
                        var connection = connectionGetter(runningNodes[i].Name, runningSlots[i]);
                        if(connection == null) throw new Exception("This circuit is not compilable: Some QBits don't reach the output.");
                        var nextNode = nodeGetter((StringName)connection["to_node"]);
                        if(nextNode is IMeasurementGate) {
                            if(runningNodes[i] != nodeIn && (lastCheckpoint == null || runningNodes[i] != lastCheckpoint)) throw new Exception("This circuit is not compilable: Measurements can only occur directly after a checkpoint.");
                            currentPart.measurements.Add(i);
                        }
                        if(runningNodes[i] is ICompileableGate gate) {
                            currentPart.compiledMatrix = gate.compile(QBitCount, new Array<int> {i}) * currentPart.compiledMatrix;
                        }
                        runningNodes[i] = nextNode;
                        runningSlots[i] = (int)connection["to_port"];
                        progress = true;
                    }
                }
                if(!progress) throw new Exception("This circuit is not compilable: Compiler ran into an endless loop.");
            }
            if(!AllAtSameCheckpoint(runningNodes)) {
                throw new Exception("This circuit is not compilable: All Qbits need to run through every checkpoint and to the output.");
            }
            circuit.parts.Add(currentPart);
            currentPart = new(QBitCount);
        }
        return circuit;
    }

    private static bool AllAtEnd(GraphNode[] nodes) {
        foreach (GraphNode node in nodes) {
            if (!(node is OutputGate)) return false;
        }
        return true;
    }

    private static bool AllAtSameCheckpoint(GraphNode[] nodes) {
        GraphNode checkpoint = null;
        foreach (GraphNode node in nodes) {
            if (!(node is ICheckpointGate)) return false;
            if (checkpoint == null) checkpoint = node;
            else if (checkpoint != node) return false;
        }
        return true;
    }

    private static bool AllAtCheckpointExcluding(GraphNode[] nodes, GraphNode exclude = null) {
        foreach (GraphNode node in nodes) {
            if (!(node is ICheckpointGate)) return false;
            if (exclude != null && node == exclude) return false;
        }
        return true;
    }

    private static bool AllInputsAtGate(GraphNode[] nodes, GraphNode gate) {
        if (!(gate is IMultiInputGate mgate)) return false;
        return mgate.GetSlotCount() == InputsAtGate(nodes, gate).Count;
    }

    private static Array<int> InputsAtGate(GraphNode[] nodes, GraphNode gate) {
        Array<int> _out = new();
        for(int i = 0; i < nodes.Length; ++i) {
            if(gate == nodes[i])_out.Add(i);
        }
        return _out;
    }
}