using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

public struct LazyMatrix: IMatrix {

	public bool EnableCaching = true;
	public static int CachingMinSize = 16;

	private IMatrix left, rightM;
	private Complex rightC;
	private LazyMatrixOperation op;

	private int calls = 0, calcs = 0;

	private Dictionary<long, Complex> cache = new Dictionary<long, Complex>();

	private int[] bitOrder;

	public LazyMatrix(IMatrix left, LazyMatrixOperation op) {
		this.left = left;
		this.rightM = left;
		this.rightC = default(Complex);
		this.op = op;
		bitOrder = new int[0];
		if (left is ClassicControlBitMatrix || rightM is ClassicControlBitMatrix) {
			EnableCaching = false;
		}
	}
	public LazyMatrix(IMatrix left, IMatrix right, LazyMatrixOperation op) {
		this.left = left;
		this.rightM = right;
		this.rightC = default(Complex);
		this.op = op;
		bitOrder = new int[0];
		if (left is ClassicControlBitMatrix || rightM is ClassicControlBitMatrix) {
			EnableCaching = false;
		}
	}

	public LazyMatrix(IMatrix left, Complex right, LazyMatrixOperation op) {
		this.left = left;
		this.rightM = left;
		this.rightC = right;
		this.op = op;
		bitOrder = new int[0];
		if (left is ClassicControlBitMatrix || rightM is ClassicControlBitMatrix) {
			EnableCaching = false;
		}
	}

	public LazyMatrix(IMatrix left, LazyMatrixOperation op, int[] bitOrder) {
		this.left = left;
		this.rightM = left;
		this.rightC = default;
		this.op = op;
		this.bitOrder = bitOrder;
		if (left is ClassicControlBitMatrix || rightM is ClassicControlBitMatrix) {
			EnableCaching = false;
		}
	}

	public Complex this[int y, int x] {
		get {
			if(EnableCaching && getN() >= CachingMinSize && op != LazyMatrixOperation.Hold && op != LazyMatrixOperation.Shuffle) {
				calls++;
				long idx = ((long)y) << 32 | (long)x; 
				if(!cache.ContainsKey(idx)) {
					calcs++;
					switch (this.op) {
						case LazyMatrixOperation.Hold: {
							cache[idx] = left[y, x];
							break;
						}
						case LazyMatrixOperation.MultComplex: {
							cache[idx] = (left[y, x] * rightC);
							break;
						}
						case LazyMatrixOperation.Transpose: {
							cache[idx] = (left[x, y]);
							break;
						}
						case LazyMatrixOperation.MultMatrix: {
							Complex value = 0;
							if(left.isSparse()) {
								var l = left.RowKeys(y);
								foreach(int i in l) {
									value += left[y, i] * rightM[i, x];
								}
							}
							else if(rightM.isSparse()) {
								var l = rightM.ColKeys(x);
								foreach(int i in l) {
									value += left[y, i] * rightM[i, x];
								}
							}
							else {
								for(int i = 0; i < left.getN(); i++){
									value += left[y, i]  * rightM[i, x];
								}
							}
							cache[idx] = value;
							
							break;
						}
						case LazyMatrixOperation.Tensor: {
							cache[idx] = left[y / rightM.getM(), x / rightM.getN()] * rightM[y % rightM.getM(), x % rightM.getN()];
							break;
						}
						default: {
							throw new ArgumentOutOfRangeException();
						}
					}
				}
				return cache[idx];
			}
			
			switch (this.op) {
				case LazyMatrixOperation.Hold: {
					return left[y, x];
				}
				case LazyMatrixOperation.Shuffle: {
					return left[getActualXY(y), getActualXY(x)];
				}
				case LazyMatrixOperation.MultComplex: {
					return (left[y, x] * rightC);
				}
				case LazyMatrixOperation.Transpose: {
					return (left[x, y]);
				}
				case LazyMatrixOperation.MultMatrix: {
					Complex value = 0;
					for(int i = 0; i < left.getN(); i++){
						value += left[y, i]  * rightM[i, x];
					}
					return value;
				}
				case LazyMatrixOperation.Tensor: {
					return left[y / rightM.getM(), x / rightM.getN()] * rightM[y % rightM.getM(), x % rightM.getN()];
				}
				default: {
					throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	public bool isSparse(){
		switch(op) {
			case LazyMatrixOperation.Hold:
			case LazyMatrixOperation.MultComplex:
			case LazyMatrixOperation.Transpose:
				return left.isSparse();
			default:
				return false;
		}
	}

	public int getN()
	{
		switch (this.op) {
			case LazyMatrixOperation.Hold: {
				return left.getN();
			}
			case LazyMatrixOperation.Shuffle: {
				return left.getN();
			}
			case LazyMatrixOperation.MultComplex: {
				return left.getN();
			}
			case LazyMatrixOperation.Transpose: {
				return left.getM();
			}
			case LazyMatrixOperation.Tensor: {
				return left.getN() * rightM.getN();
			}
			case LazyMatrixOperation.MultMatrix: {
				return rightM.getN();
			}
			default: {
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public int getM()
	{
		switch (this.op) {
			case LazyMatrixOperation.Hold: {
				return left.getM();
			}
			case LazyMatrixOperation.Shuffle: {
				return left.getM();
			}
			case LazyMatrixOperation.MultComplex: {
				return left.getM();
			}
			case LazyMatrixOperation.Transpose: {
				return left.getN();
			}
			case LazyMatrixOperation.Tensor: {
				return left.getM() * rightM.getM();
			}
			case LazyMatrixOperation.MultMatrix: {
				return left.getM();
			}
			default: {
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public LazyMatrix Transposed() {
		return new LazyMatrix(this, LazyMatrixOperation.Transpose);
	}

	public static LazyMatrix operator *(LazyMatrix m1, Complex c1) {
		return new LazyMatrix(m1, c1, LazyMatrixOperation.MultComplex);
	}

	public static LazyMatrix operator *(Complex c1, LazyMatrix m1) {
		return m1 * c1;
	}

	public static LazyMatrix operator *(LazyMatrix m1, IMatrix m2) {
		return new LazyMatrix(m1, m2, LazyMatrixOperation.MultMatrix);
	}

	// (Mis)use of the XOR operator to signify Tensorproduct
	public static LazyMatrix operator ^(LazyMatrix m1, IMatrix m2) {
		return new LazyMatrix(m1, m2, LazyMatrixOperation.Tensor);
	}

	public override string ToString() {
		string _out = $"{getM()} by {getN()} Matrix: [\n";
		for(int i = 0; i < getM(); i++) {
			for(int j = 0; j < getN(); j++) {
				_out += $"m{i}{j} = {this[i, j] + (j == getN() - 1 ? "\n": ", ")}";
			}
		}
		return _out + "]";
	}

	public string test() {
		string _out = $"{getM()} by {getN()} Matrix: [\n";
		for(int i = 0; i < getM(); i++) {
			for(int j = 0; j < getN(); j++) {
				_out += $"{this[i, j].real + (j == getN() - 1 ? "\n": ", ")}";
			}
		}
		return _out + "]";
	}

	public static Vector operator *(LazyMatrix m1, Vector v1) {
		if(m1.getN() != v1.length) {
			throw new ArgumentException("Matrix and Vector dimensions differ, unable to multiply");
		}
		if(v1.transposed) {
			throw new ArgumentException("Vector is transposed, unable to multiply");
		}


		Vector _out = new(m1.getM());

		if(m1.isSparse()) {
			for(int i = 0; i < m1.getM(); i++) {
				var l = m1.RowKeys(i);
				Complex val = 0;
				foreach(int j in l) {
					if(v1[j].Abs2() == 0) continue;
					val += m1[i,j] * v1[j];
				}
				_out[i] = val;
			}
		}else {
			for(int i = 0; i < m1.getM(); i++) {
				Complex val = 0;
				for(int j = 0; j < m1.getN(); j++) {
					if(v1[j].Abs2() == 0) continue;
					val += m1[i,j] * v1[j];
				}
				_out[i] = val;
			}
		}
		return _out;
	}

	public static explicit operator LazyMatrix(StructMatrix m1) {
		return new LazyMatrix(m1, LazyMatrixOperation.Hold);
	}
	
	public void EvalCache() {
		GD.Print($"{getM()} * {getN()}: {calcs}/{calls} => {((double)calcs)/calls}");
		if(left is LazyMatrix matrix) matrix.EvalCache();
		if(rightM != null && rightM != left) ((LazyMatrix)rightM).EvalCache();
	}

    public List<int> RowKeys(int row)
    {
        return op == LazyMatrixOperation.Transpose ? left.ColKeys(row) : left.RowKeys(row);
    }

    public List<int> ColKeys(int col)
    {
        return op == LazyMatrixOperation.Transpose ? left.RowKeys(col) : left.ColKeys(col);
    }

	private int getActualXY(int xOrY) {
		int res = 0;
		for(int i = 0; i < bitOrder.Length; ++i) {
			res |= ((xOrY & (1 << bitOrder[i])) >> bitOrder[i]) << i;
		}
		return res;
	}

	public byte[] ToByteArray() {
		byte[] _out = new byte[8 + 16*getM()*getN()];
		Buffer.BlockCopy(Helpers.BytesFromInt(getM()), 0, _out, 0, 4);
		Buffer.BlockCopy(Helpers.BytesFromInt(getN()), 0, _out, 4, 4);
		for(int y = 0; y < getM(); ++y) {
			for(int x = 0; x < getN(); ++x) {
				var bytes = Helpers.BytesFromDoubles(this[y,x]);
				Buffer.BlockCopy(bytes, 0, _out, 8 + x * 16 + y * 16 * getN(), bytes.Length);
			}
		}
		return _out;
	}

	public static LazyMatrix FromByteArray(byte[] bytes) {
		return new LazyMatrix(StructMatrix.FromByteArray(bytes), LazyMatrixOperation.Hold);
	}
}

public enum LazyMatrixOperation {
	MultComplex,
	MultMatrix,
	Tensor,
	Transpose,
	Hold,
	Shuffle
}
