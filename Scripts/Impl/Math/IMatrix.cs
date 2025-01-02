using Godot;
using System;
using System.Collections.Generic;

public interface IMatrix {
    public unsafe Complex this[int y, int x] {
        get;
    }

    public int getN();
    public int getM();

    public bool isSparse();

    public List<int> RowKeys(int row);
    public List<int> ColKeys(int col);
}