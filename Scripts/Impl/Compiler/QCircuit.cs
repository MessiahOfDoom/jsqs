using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;

public record QCircuit {

    public delegate Vector InputGetter();
    public delegate MeasurementGate MeasurementGateGetter(StringName name);

    public List<QCircuitPart> parts = new();

    public List<Vector> inputs = new();

    public List<int> measurementCheckpoints = new();

    public Godot.Collections.Dictionary<StringName, int> measurementGatesToBits = new();
    public int[] bits;

    public InputGetter inputGetter;
    public MeasurementGateGetter measurementGateGetter = null;

    public Godot.Collections.Dictionary<string, int> CheckpointNameToPartIndex = new();

    public Vector Run() {
        try {
            return RunWithInput(inputGetter());
        } catch(Exception) {
            return default;
        }
    }

    public Vector RunWithInput(Vector input) {
        var output = input;
        for(int i = 0; i < parts.Count; ++i) {
            inputs.Add(output);
            if(parts[i].measurements.Count > 0) measurementCheckpoints.Add(i);
            output = parts[i].RunWithInput(output, this);
        }
        inputs.Add(output);
        return output;
    }

    public Vector RerunFromPart(int part) {
        if(inputs.Count == 0) return default;
        var output = inputs[part];
        if(part < parts.Count) {
            inputs.RemoveRange(part, inputs.Count - part);
        }
        for(int i = part; i < parts.Count; ++i) {
            inputs.Add(output);
            output = parts[i].RunWithInput(output, this);
        }
        inputs.Add(output);
        return output;
    }

    public Vector CalculateProbabilityVector() {
        try {
            return CalculateProbabilityVector(inputGetter());
        } catch (Exception) {
            return default;
        }
    }
    public Vector CalculateProbabilityVector(Vector input) {
        List<StringName> AllGates = new();
        List<int> AllBits = new();
        foreach(var part in parts) {
            AllGates.AddRange(part.measurementGates);
        }
        foreach(var gate in AllGates) {
            AllBits.Add(0);
        }

        var actualInput = input;
        var nextPart = 0;
        for(int i = 0; i < parts.Count; ++i) {
            if(parts[i].measurements.Count > 0) break;
            actualInput = parts[i].RunWithInput(actualInput, this);
            nextPart = i + 1;
        }
        if (nextPart == parts.Count) return actualInput;
        var output = new Vector(input.length);

        do {
            var currentRun = actualInput;
            var totalChance = 1.0;

            foreach(var pair in measurementGatesToBits) {
                bits[pair.Value] = AllBits[AllGates.IndexOf(pair.Key)];
            }

            for(var partIdx = nextPart; partIdx < parts.Count; ++partIdx) {
                currentRun = parts[partIdx].CalculateStateVector(currentRun, this, AllGates, AllBits, ref totalChance);
                if(totalChance == 0) break;
            }
            if(totalChance != 0) {
                var mult = 1 / (totalChance * totalChance);
                for(int i = 0; i < output.length; ++i){
                    output[i] += currentRun[i].Abs2() * mult; 
                }
            }
            for(int i = 0; i < AllBits.Count; ++i) {
                if(AllBits[i] == 0) {
                    AllBits[i] = 1;
                    break;
                }else {
                    AllBits[i] = 0;
                }
            }
        } while(AllBits.Contains(1));
        return output;
    }

    public System.Collections.Generic.Dictionary<int, int> MonteCarlo(int runs, int fromCheckpoint) {
        GD.Randomize();
        
        var _out = new System.Collections.Generic.Dictionary<int, int>();

        var random = Helpers.WeightedRandomFromProbabilityVector(CalculateProbabilityVector());
        for(int i = 0; i < runs; ++i) {
            var res = random.Draw();
            _out[res] = _out.ContainsKey(res) ? ++_out[res] : 1;
        }
        /*var actualFrom = -1;
        for(int i = 0; i < measurementChepoints.Count; ++i) {
            if(measurementChepoints[i] < fromCheckpoint) continue;
            actualFrom = measurementChepoints[i];
            break;
        }
        if(actualFrom == -1) {
            var output = inputs[inputs.Count - 1];
            var random = Helpers.WeightedRandomFromVector(output);
            for(int i = 0; i < runs; ++i) {
                var res = random.Draw();
                _out[res] = _out.ContainsKey(res) ? ++_out[res] : 1;
            }
        }else {
            var input = inputs[actualFrom];
            for(int i = 0; i < runs; ++i) {
                var output = input;
                for(int j = actualFrom; j < parts.Count; ++j){
                    output = parts[j].RunWithInput(output, this);
                }
                var res = Helpers.WeightedRandomFromVector(output).Draw();
                _out[res] = _out.ContainsKey(res) ? ++_out[res] : 1;
            }
        }*/

        return _out;
    }

    public int GetMatrixIndexByBits(int[] b) {
        var res = 0;
        for(int i = 0; i < b.Length; ++i) {
            res |= bits[b[i]] << i;
        }
        return res;
    }
}

public record QCircuitPart {
    public LazyMatrix compiledMatrix;
    public Array<int> measurements = new();

    public Array<StringName> measurementGates = new();

    public QCircuitPart(int QBitCount) {
        compiledMatrix = GateBuilder.Identity(QBitCount);        
    }

    public Vector RunWithInput(Vector input, QCircuit c) {
        var measuredInput = input;
        for(int i = 0; i < measurements.Count; ++i) {
            measuredInput = WithRandomMeasurement(measuredInput, measurements[i], measurementGates[i], c);
        }
        return compiledMatrix * measuredInput;
    }

    public Vector CalculateStateVector(Vector input, QCircuit c, List<StringName> gates, List<int> bits, ref double totalChance) {
        var measuredInput = input;
        for(int i = 0; i < measurements.Count; ++i) {
            var idx = gates.IndexOf(measurementGates[i]);
            measuredInput = CalculateStateVectorComponent(measuredInput, measurements[i], bits[idx], ref totalChance);
            if(totalChance == 0) return input;
        }
        return compiledMatrix * measuredInput;
    }

    private Vector CalculateStateVectorComponent(Vector input, int QBit, int measurement, ref double totalChance) {
        var chance = MeasurementChance(input, QBit, measurement);
        double mult = 1 / Math.Sqrt(chance);
        totalChance *= mult;
        if(chance == 0) {
            totalChance = 0;
            return new(input.length);
        }
        var res = new Vector(input.length);
        for(int i = 0; i < input.length; ++i) {
            if((((i & (1 << QBit)) >> QBit) ^ measurement) == 0) {
                res[i] = input[i] * mult;
            }
        }
        return res;
    }

    private Vector WithRandomMeasurement(Vector input, int QBit, StringName gate, QCircuit c) {
        double chance0 = MeasurementChance(input, QBit, 0);
        double chance1 = 1 - chance0;
        var rand = new WeightedRandom<int>();
        rand.AddItem(0, chance0);
        rand.AddItem(1, chance1);
        var choice = rand.Draw();
        if(c.measurementGateGetter != null)
            c.measurementGateGetter(gate).SetMeasurement(choice);
        if(c.measurementGatesToBits.ContainsKey(gate)) {
            c.bits[c.measurementGatesToBits[gate]] = choice;
        }
        double mult = 1 / Math.Sqrt(choice == 0 ? chance0 : chance1);
        
        var res = new Vector(input.length);
        for(int i = 0; i < input.length; ++i) {
            if((((i & (1 << QBit)) >> QBit) ^ choice) == 0) {
                res[i] = input[i] * mult;
            }
        }
        return res;
    }

    private double MeasurementChance(Vector input, int QBit, int measurement) {
        double res = 0;
        for(int i = 0; i < input.length; ++i) {
            if((((i & (1 << QBit)) >> QBit) ^ measurement) == 0) {
                res += input[i].Abs2();
            }
        }
        return res;
    }

    
}