using Godot;
using System;

public partial class Matrix
{
	
	private Complex[,] values;
	private int m = 0, n = 0;

	public Matrix() {
		values = new Complex[0, 0];
	}

	public Matrix(int m, int n) {
		values = new Complex[m, n];
        for(int i = 0; i < m; i++){
            for(int j = 0; j < n; j++){
                values[i, j] = 0;
            }
        }
		this.m = m;
		this.n = n;
	}

	public Matrix Clone() {
		Matrix _out = new Matrix(m, n);
		for(int i = 0; i < m; i++) {
			for(int j = 0; j < n; j++) {
				_out[i, j] = this[i, j].Clone();
			}
		}
		return _out;
	}

	public Matrix Transposed() {
		Matrix _out = new Matrix(n, m);
		for(int i = 0; i < m; i++) {
			for(int j = 0; j < n; j++) {
				_out[j, i] = this[i, j].Clone();
			}
		}
		return _out;
	}

	public Complex this[int y, int x] {
		get => values[y, x];
		set => values[y, x] = value;
	}

	public static Matrix operator *(Matrix m1, Complex c1) {
		Matrix _out = new Matrix(m1.m, m1.n);
		for(int i = 0; i < m1.m; i++) {
			for(int j = 0; j < m1.n; j++) {
				_out[i, j] = m1[i, j] * c1;
			}
		}
		return _out;
	}

    public static Matrix operator *(Complex c1, Matrix m1) {
		return m1 * c1;
	}

	public static Matrix operator *(Matrix m1, Matrix m2) {
		if(m1.n != m2.m) {
			throw new ArgumentException("Matrix dimensions differ, unable to multiply");
		}
		Matrix _out = new Matrix(m1.m, m2.n);
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
	public static Matrix operator ^(Matrix m1, Matrix m2) {
		Matrix _out = new Matrix(m1.m * m2.m, m1.n * m2.n);
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
		string _out = "[\n";
		for(int i = 0; i < m; i++) {
			for(int j = 0; j < n; j++) {
				_out += $"m{i}{j} = {this[i, j] + (j == n - 1 ? "\n": ", ")}";
			}
		}
		return _out + "]";
	}

    public static Vector operator *(Matrix m1, Vector v1) {
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

}
