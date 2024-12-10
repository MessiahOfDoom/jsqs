using Godot;
using System;
using System.Collections.Generic;

public struct SparseMatrix: IMatrix {

    private Dictionary<long, Complex> dict = new();

    private int m = 0, n = 0;

    public SparseMatrix(int m, int n) {
        this.m = m;
        this.n = n;
    }

    public unsafe Complex this[int y, int x] {
        get {
            if(y < 0 || y >= m|| x < 0 || x > n) throw new ArgumentException("Out of bounds");
            return dict.GetValueOrDefault((((long)y) << 32) | (long)x, new Complex());
        }
        set {
            if(y < 0 || y >= m|| x < 0 || x > n) throw new ArgumentException("Out of bounds");
            dict[(((long)y) << 32) | (long)x] = value;
        }
    }

    public int getM() {
        return m;
    }

    public int getN(){
        return n;
    }

}