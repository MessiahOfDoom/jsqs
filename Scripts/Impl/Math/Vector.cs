using Godot;
using System;

public record Vector
{
    private Complex[] values;
    public int length = 0;

    public bool transposed = false;

    public Vector() {
        values = new Complex[0];
        length = 0;
    }

    public Vector(int size) {
        values = new Complex[size];
        for(int i = 0; i < size; i++) {
            values[i] = 0;
        }
        length = size;
    }

    public Vector(Vector toClone) {
        values = new Complex[toClone.length];
        length = toClone.length;
        for(int i = 0; i < length; ++i) {
            values[i] = toClone[i].Clone();
        }
    }

    public Complex this[int index] {
        get => values[index];
        set => values[index] = value;
    }

    public void Transpose() {
        transposed = !transposed;
    }

    public static Vector operator +(Vector v1, Vector v2) {
        if(v1.length != v2.length) {
            throw new ArgumentException("Vectors can only be added when they're the same length");
        }	
        Vector _out = new Vector(v1.length);
        for(int i = 0; i < v1.length; i++) {
            _out[i] = v1[i] + v2[i];
        }
        return _out;
    }

    public static Vector operator -(Vector v1, Vector v2) {
        if(v1.length != v2.length) {
            throw new ArgumentException("Vectors can only be subtracted when they're the same length");
        }	
        Vector _out = new Vector(v1.length);
        for(int i = 0; i < v1.length; i++) {
            _out[i] = v1[i] - v2[i];
        }
        return _out;
    }

    public static Vector operator *(Vector v1, Vector v2) {
        if(v1.length != v2.length) {
            throw new ArgumentException("Vectors can only be multiplied when they're the same length");
        }	
        Vector _out = new Vector(v1.length);
        for(int i = 0; i < v1.length; i++) {
            _out[i] = v1[i] * v2[i];
        }
        return _out;
    }

    public static Vector operator /(Vector v1, Vector v2) {
        if(v1.length != v2.length) {
            throw new ArgumentException("Vectors can only be divided when they're the same length");
        }	
        Vector _out = new Vector(v1.length);
        for(int i = 0; i < v1.length; i++) {
            _out[i] = v1[i] / v2[i];
        }
        return _out;
    }

    public static Vector operator *(Vector v1, Complex c1) {
        Vector _out = new Vector(v1.length);
        for(int i = 0; i < v1.length; i++) {
            _out[i] = v1[i] * c1;
        }
        return _out;
    }

    public static Vector operator -(Vector v1, Complex c1) {
        Vector _out = new Vector(v1.length);
        for(int i = 0; i < v1.length; i++) {
            _out[i] = v1[i] / c1;
        }
        return _out;
    }

    // (Mis)use of the XOR operator to signify Tensorproduct
    public static Vector operator ^(Vector v1, Vector v2) {
        Vector _out = new Vector(v1.length * v2.length);
        for(int i = 0; i < v1.length; i++) {
            for(int j = 0; j < v2.length; j++) {
                _out[i*v2.length + j] = v1[i] * v2[j];
            }
        }
        return _out;
    }

    public override string ToString() {
        string _out = "[\n";
        for(int i = 0; i < length; i++){
            _out += this[i] + (i == length - 1 ? "\n" : ", " + (transposed ? "" : "\n"));
        }
        return _out + "]";
    }

    public static implicit operator Vector(double[] d) {
        if(d.Length % 2 != 0) throw new ArgumentException("Input Array has an invalid Length");
        Vector _out = new(d.Length / 2);
        for(int i = 0; i < d.Length / 2; ++i) {
            _out[i] = new(d[i * 2], d[i * 2 + 1]);
        }
        return _out;
    }

    public static implicit operator double[](Vector v) {
        double[] _out = new double[v.length * 2];
        for(int i = 0; i < v.length; ++i) {
            _out[i * 2] = v[i].real;
            _out[i * 2 + 1] = v[i].imag;
        }
        return _out;
    }

    public double Abs2() {
        double total = 0;
        foreach(Complex c in values) {
            total += c.Abs2();
        }
        return total;
    }
}
