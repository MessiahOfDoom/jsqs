

using System;
using System.Collections.Generic;
using Godot;

public struct ClassicControlBitMatrix : IMatrix
{
    
    public delegate int GetMatrixIndexByBits(int[] bits);

    private GetMatrixIndexByBits bitGetter;
    private IMatrix[] matrices;
    private int[] bits;

    public ClassicControlBitMatrix(IMatrix[] m, int[] b, GetMatrixIndexByBits getter) {
        matrices = m;
        if(matrices.Length == 0) throw new ArgumentException("Cannot construct a classic control bit matrix without at least one option");
        int M = matrices[0].getM(); 
        int N = matrices[0].getN();
        foreach(var mat in matrices) {
            if(mat.getM() != M || mat.getN() != N) throw new ArgumentException("Cannot have multiple matrices of different dimensions");
        }
        bits = b;
        if(1 << bits.Length != matrices.Length) throw new ArgumentException("Incompatible number of matrices and bits");
        bitGetter = getter;
    }

    public Complex this[int y, int x] {
        get {
            if(bitGetter != null) {
                return matrices[bitGetter(bits)][y, x];
            }
            return 0;
        }
    }

    public List<int> ColKeys(int col)
    {
        throw new System.NotImplementedException("Classic control bit matrices are not sparse.");
    }

    public int getM()
    {
        return matrices[0].getM();
    }

    public int getN()
    {
        return matrices[0].getN();
    }

    public bool isSparse()
    {
        return false;
    }

    public List<int> RowKeys(int row)
    {
        throw new System.NotImplementedException("Classic control bit matrices are not sparse.");
    }
}