using Godot;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public partial class GateBuilder
{
	private static Dictionary<int, LazyMatrix> Hadamards = new();

	static GateBuilder(){
		Hadamard_1Bit = new StructMatrix(2, 2);
		Hadamard_1Bit[0, 0] = Consts.SQRT_TWO_HALVES;
		Hadamard_1Bit[0, 1] = Consts.SQRT_TWO_HALVES;
		Hadamard_1Bit[1, 0] = Consts.SQRT_TWO_HALVES;
		Hadamard_1Bit[1, 1] = -Consts.SQRT_TWO_HALVES;
		Hadamard_1Bit_Lazy = (LazyMatrix)Hadamard_1Bit;
		
		Hadamards[1] = Hadamard_1Bit_Lazy;
		for(int i = 2; i <= Consts.NUM_QBITS; i++) {
			int n1 = i / 2;
			int n2 = i - n1;
			Hadamards[i] = Hadamards[n1] ^ Hadamards[n2];
		}

	}

	public static LazyMatrix Identity(int qbits) {
		int size = (int)Math.Pow(2,qbits);
		SparseMatrix _out = new(size, size);
		for(int i = 0; i < size; i++){
			_out[i, i] = 1;
		}
		return new LazyMatrix(_out, LazyMatrixOperation.Hold);
	}
	private static StructMatrix Hadamard_1Bit;
	private static LazyMatrix Hadamard_1Bit_Lazy;
	public static LazyMatrix Hadamard(int qbits) {
		if(qbits <= Consts.NUM_QBITS) {
			return Hadamards[qbits];
		}
		return Hadamard(qbits / 2) ^ Hadamard(qbits - (qbits / 2));
	}

	public static LazyMatrix PauliX() {
		SparseMatrix _out = new(2, 2);
		_out[0, 1] = 1;
		_out[1, 0] = 1;
		return new LazyMatrix(_out, LazyMatrixOperation.Hold);
	}

	public static LazyMatrix PauliY() {
		SparseMatrix _out = new(2, 2);
		_out[0, 1] = new(0, -1);
		_out[1, 0] = new(0, 1);
		return new LazyMatrix(_out, LazyMatrixOperation.Hold);
	}

	public static LazyMatrix PauliZ() {
		SparseMatrix _out = new(2, 2);
		_out[0, 0] = 1;
		_out[1, 1] = -1;
		return new LazyMatrix(_out, LazyMatrixOperation.Hold);
	}

	public static LazyMatrix CNot() {
		var _out = new SparseMatrix(4,4);
		_out[0,0] = 1;
		_out[1,1] = 1;
		_out[2,3] = 1;
		_out[3,2] = 1;
		return new LazyMatrix(_out, LazyMatrixOperation.Hold);
	}

	public static LazyMatrix Swap() {
		var _out = new SparseMatrix(4,4);
		_out[0,0] = 1;
		_out[1,2] = 1;
		_out[2,1] = 1;
		_out[3,3] = 1;
		return new LazyMatrix(_out, LazyMatrixOperation.Hold);
	}

	public static LazyMatrix Toffoli() {
		var _out = new SparseMatrix(8,8);
		for(int i = 0; i < 6; ++i) {
			_out[i,i] = 1;
		}
		_out[6,7] = 1;
		_out[7,6] = 1;
		return new LazyMatrix(_out, LazyMatrixOperation.Hold);
	}

	public static LazyMatrix RY(double alpha) {
		var _out = new SparseMatrix(2,2);
		_out[0,0] = Math.Cos(alpha);
		_out[1,0] = Math.Sin(alpha);
		_out[0,1] = - Math.Sin(alpha);
		_out[1,1] = Math.Cos(alpha);
		return new LazyMatrix(_out, LazyMatrixOperation.Hold);
	}

	public static LazyMatrix Controlled(IMatrix matrix) {
		if(matrix == null) throw new ArgumentNullException("Tried constructing a controlled Matrix without providing the matrix.");
		if(matrix.getM() == 2 && matrix.getN() == 2) {
			var _out = new SparseMatrix(4,4);
			_out[0,0] = 1;
			_out[1,1] = 1;
			_out[2,2] = matrix[0, 0];
			_out[2,3] = matrix[0, 1];
			_out[3,2] = matrix[1, 0];
			_out[3,3] = matrix[1, 1];
			return new LazyMatrix(_out, LazyMatrixOperation.Hold);
		}
		throw new NotImplementedException("Controlled gates are only implemented for 2 QBits as of now.");
	}
}
