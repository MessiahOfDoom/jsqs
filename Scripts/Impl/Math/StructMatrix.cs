using Godot;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

public struct StructMatrix: IMatrix {
	private int m = 0, n = 0;
	private double[] data;

	public StructMatrix(int m, int n) {
		this.m = m;
		this.n = n;
		data = new double[m*n*2];
	}

	public StructMatrix Clone() {
		return this with {data=(double[])data.Clone()};
	}

	public unsafe Complex this[int y, int x] {
		get {
			fixed(double* p = &data[2 * (y * n + x)]) {
				return p;
			}
		}
		set {
			this[y, x, 0] = value.real;
			this[y, x, 1] = value.imag;
		}
	}

	public double this[int y, int x, int z] {
		get => data[2 * (y * n + x) + z];
		set => data[2 * (y * n + x) + z] = value;
	}

	public StructMatrix Transposed() {
		var _out = new StructMatrix(m, n);
		for(int i = 0; i < m; i++) {
			for(int j = 0; j < n; j++) {
				_out[j, i, 0] = this[i, j, 0];
				_out[j, i, 1] = this[i, j, 1];
			}
		}
		return _out;
	}

	public static StructMatrix operator *(StructMatrix m1, Complex c1) {
		StructMatrix _out = new StructMatrix(m1.m, m1.n);
		for(int i = 0; i < m1.m; i++) {
			for(int j = 0; j < m1.n; j++) {
				_out[i, j] = m1[i, j] * c1;
			}
		}
		return _out;
	}

	public static StructMatrix operator *(Complex c1, StructMatrix m1) {
		return m1 * c1;
	}

	public static StructMatrix operator *(StructMatrix m1, StructMatrix m2) {
		if(m1.n != m2.m) {
			throw new ArgumentException("Matrix dimensions differ, unable to multiply");
		}
		StructMatrix _out = new StructMatrix(m1.m, m2.n);
		for(int i = 0; i < _out.m; i++) {
			for(int j = 0; j < _out.n; j++) {
				Complex value = 0;
				for(int k = 0; k < m1.n; k++) {
					value += m1[i, k] * m2[k, j];
				}  
				_out[i, j] = value;
			}
		}
		return _out;
	}

	// (Mis)use of the XOR operator to signify Tensorproduct
	public static StructMatrix operator ^(StructMatrix m1, StructMatrix m2) {
		StructMatrix _out = new StructMatrix(m1.m * m2.m, m1.n * m2.n);
		for(int i = 0; i < m1.m; i++) {
			for(int j = 0; j < m1.n; j++) {
				for(int k = 0; k < m2.m; k++) {
					for(int l = 0; l < m2.n; l++) {
						_out[k + i * m2.m, l + j * m2.n] = m1[i, j] * m2[k, l];
					}   
				}
			}
		}

		return _out;
	}

	public override string ToString() {
		string _out = $"{m} by {n} Matrix: [\n";
		for(int i = 0; i < m; i++) {
			for(int j = 0; j < n; j++) {
				_out += $"m{i}{j} = {this[i, j] + (j == n - 1 ? "\n": ", ")}";
			}
		}
		return _out + "]";
	}

	public static Vector operator *(StructMatrix m1, Vector v1) {
		if(m1.n != v1.length) {
			throw new ArgumentException("Matrix and Vector dimensions differ, unable to multiply");
		}
		if(v1.transposed) {
			throw new ArgumentException("Vector is transposed, unable to multiply");
		}

		Vector _out = new Vector(m1.m);
		for(int i = 0; i < m1.m; i++) {
			Complex val = 0;
			for(int j = 0; j < m1.n; j++) {
				val += m1[i,j] * v1[j];
			}
			_out[i] = val;
		}
		return _out;
	}

	public byte[] ToByteArray() {
		return Helpers.Append(Helpers.Append(Helpers.BytesFromInt(m), Helpers.BytesFromInt(n)), Helpers.BytesFromDoubles(data));
	}

	public static StructMatrix FromByteArray(byte[] bytes) {
		if(bytes.Length < 8) throw new Exception("Too few bytes to parse as a matrix.");
		var m = Helpers.IntFromBytes(Helpers.GetByteSubArray(bytes, 0, 4));
		var n = Helpers.IntFromBytes(Helpers.GetByteSubArray(bytes, 4, 4));
		var _out = new StructMatrix(m, n);
		if(bytes.Length < 8 + (m*n*16)) throw new Exception("Too few bytes to parse as a matrix.");
		_out.data = Helpers.DoublesFromBytes(Helpers.GetByteSubArray(bytes, 8, m*n*16));
		return _out;
	}

	public void WriteToFile(String path) {
		File.WriteAllBytes(path, ToByteArray());
	}

	public static StructMatrix ReadFromFile(String path) {
		return StructMatrix.FromByteArray(File.ReadAllBytes(path));
	}

    public int getN()
    {
        return n;
    }

    public int getM()
    {
        return m;
    }

	public bool isSparse() {
		return false;
	}

    public List<int> RowKeys(int row)
    {
        throw new NotImplementedException("Struct Matrices are not sparse");
    }

    public List<int> ColKeys(int col)
    {
        throw new NotImplementedException("Struct Matrices are not sparse");
    }

    public static bool operator ==(StructMatrix left, StructMatrix right) {
		if((left.m != right.m) || (left.n != right.n)) return false;
		for(int i = 0; i < left.data.Length; i++){
			if(left.data[i] != right.data[i])return false;
		}
		return true;
	}

	public static bool operator !=(StructMatrix left, StructMatrix right) {
		return !(left == right);
	}

	public override bool Equals(object obj) {
		return (obj is StructMatrix matrix) && this == matrix;
	}

	public override int GetHashCode() {
		return base.GetHashCode();
	}
}
