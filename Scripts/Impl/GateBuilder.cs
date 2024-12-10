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
			//GD.Print(Hadamards[i].getM());
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


}
