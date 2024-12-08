using Godot;
using System;

public interface IMatrix {
    public unsafe Complex this[int y, int x] {
        get;
    }

    public int getN();
    public int getM();
}