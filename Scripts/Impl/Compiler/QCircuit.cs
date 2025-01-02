using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public record QCircuit {
    public List<QCircuitPart> parts = new();

    public Vector RunWithInput(Vector input) {
        var output = input;
        for(int i = 0; i < parts.Count; ++i) {
            output = parts[i].RunWithInput(output);
        }
        return output;
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
        return compiledMatrix * input;
    }
}