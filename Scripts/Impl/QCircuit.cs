using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public record QCircuit {
    public Vector Input;
    public List<QCircuitPart> parts = new();

    public QCircuit(Vector input) {
        Input = input;
    }
}

public record QCircuitPart {
    public LazyMatrix compiledMatrix;
    public Array<int> measurements = new();

    public QCircuitPart(int QBitCount) {
        compiledMatrix = GateBuilder.Identity(QBitCount);        
    }
}