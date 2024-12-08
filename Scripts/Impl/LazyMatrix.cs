using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;

public struct LazyMatrix: IMatrix {

    public static bool EnableCaching = true;
    public static int CachingMinSize = 16, CachingMaxSize = 4096;

    private IMatrix left, rightM;
    private Complex rightC;
    private LazyMatrixOperation op;

    private Dictionary<long, Complex> cache = new Dictionary<long, Complex>();

    public LazyMatrix(IMatrix left, LazyMatrixOperation op) {
        this.left = left;
        this.rightM = left;
        this.rightC = default(Complex);
        this.op = op;
    }
    public LazyMatrix(IMatrix left, IMatrix right, LazyMatrixOperation op) {
        this.left = left;
        this.rightM = right;
        this.rightC = default(Complex);
        this.op = op;
    }

    public LazyMatrix(IMatrix left, Complex right, LazyMatrixOperation op) {
        this.left = left;
        this.rightM = left;
        this.rightC = right;
        this.op = op;
    }

    public Complex this[int y, int x] {
        get {
            if(EnableCaching && getN() >= CachingMinSize && getN() <= CachingMaxSize) {
                long idx = ((long)y) << 32 | (long)x; 
                if(!cache.ContainsKey(idx)) {
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
                            for(int i = 0; i < left.getN(); i++){
                                value += left[y, i]  * rightM[i, x];
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

    public int getN()
    {
        switch (this.op) {
            case LazyMatrixOperation.Hold: {
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

	public static Vector operator *(LazyMatrix m1, Vector v1) {
		if(m1.getN() != v1.length) {
			throw new ArgumentException("Matrix and Vector dimensions differ, unable to multiply");
		}
		if(v1.transposed) {
			throw new ArgumentException("Vector is transposed, unable to multiply");
		}

		Vector _out = new Vector(m1.getM());
		for(int i = 0; i < m1.getM(); i++) {
			Complex val = 0;
			for(int j = 0; j < m1.getN(); j++) {
				if(v1[j].Abs2() == 0) continue;
                val += m1[i,j] * v1[j];
			}
			_out[i] = val;
		}
		return _out;
	}

    public static explicit operator LazyMatrix(StructMatrix m1) {
        return new LazyMatrix(m1, LazyMatrixOperation.Hold);
    }
}

public enum LazyMatrixOperation {
    MultComplex,
    MultMatrix,
    Tensor,
    Transpose,
    Hold
}