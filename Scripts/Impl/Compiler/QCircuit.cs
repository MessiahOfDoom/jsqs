using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public record QCircuit {
    public List<QCircuitPart> parts = new();

    public List<Vector> inputs = new();

    public List<int> measurementChepoints = new();

    public Vector RunWithInput(Vector input) {
        var output = input;
        for(int i = 0; i < parts.Count; ++i) {
            inputs.Add(output);
            if(parts[i].measurements.Count > 0) measurementChepoints.Add(i);
            output = parts[i].RunWithInput(output);
        }
        inputs.Add(output);
        return output;
    }

    public System.Collections.Generic.Dictionary<int, int> MonteCarlo(int runs, int fromCheckpoint) {
        GD.Randomize();
        
        var _out = new System.Collections.Generic.Dictionary<int, int>();
        if(inputs.Count == 0) throw new Exception("The circuit must run at least once before calling this method");
        var actualFrom = -1;
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
                    output = parts[j].RunWithInput(output);
                }
                var res = Helpers.WeightedRandomFromVector(output).Draw();
                _out[res] = _out.ContainsKey(res) ? ++_out[res] : 1;
            }
        }

        return _out;
    }
}

public record QCircuitPart {
    public LazyMatrix compiledMatrix;
    public Array<int> measurements = new();

    public QCircuitPart(int QBitCount) {
        compiledMatrix = GateBuilder.Identity(QBitCount);        
    }

    public Vector RunWithInput(Vector input) {
        //TODO Measurements
        GD.Print(compiledMatrix);
        return compiledMatrix * input;
    }
}