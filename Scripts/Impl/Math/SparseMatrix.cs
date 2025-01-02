using Godot;
using System;
using System.Collections.Generic;

public struct SparseMatrix: IMatrix {

    private Dictionary<long, Complex> dict = new();
    private List<int>[] keysByRow;
    private List<int>[] keysByCol;
    private readonly int m = 0;
    private readonly int n = 0;

    public SparseMatrix(int m, int n) {
        this.m = m;
        this.n = n;
        keysByRow = Helpers.ArrayWithValue(m, () => new List<int>());
        keysByCol = Helpers.ArrayWithValue(m, () => new List<int>());
    }

    public unsafe Complex this[int y, int x] {
        get {
            if(y < 0 || y >= m|| x < 0 || x > n) throw new ArgumentException("Out of bounds");
            return dict.GetValueOrDefault((((long)y) << 32) | (long)x, new Complex());
        }
        set {
            if(y < 0 || y >= m|| x < 0 || x > n) throw new ArgumentException("Out of bounds");
            long key = (((long)y) << 32) | (long)x;
            dict[key] = value;
            keysByCol[x].Add(y);
            keysByRow[y].Add(x);
        }
    }

    public int getM() {
        return m;
    }

    public int getN(){
        return n;
    }

    public bool isSparse(){
        return true;
    }

    public List<int> RowKeys(int row)
    {
        return keysByRow[row];
    }

    public List<int> ColKeys(int col)
    {
        return keysByCol[col];
    }
}