using Godot;
using System;
using System.Runtime.InteropServices;

public partial class GateBuilder
{
    

    static GateBuilder(){
        Hadamard_1Bit = new StructMatrix(2, 2);
        Hadamard_1Bit[0, 0] = Consts.SQRT_TWO_HALVES;
        Hadamard_1Bit[0, 1] = Consts.SQRT_TWO_HALVES;
        Hadamard_1Bit[1, 0] = Consts.SQRT_TWO_HALVES;
        Hadamard_1Bit[1, 1] = -Consts.SQRT_TWO_HALVES;
        Hadamard_1Bit_Lazy = (LazyMatrix)Hadamard_1Bit;
    }

    public static LazyMatrix Identity(int qbits) {
        GD.Print(Math.Pow(2, qbits));
        int size = (int)Math.Pow(2,qbits);
        StructMatrix _out = new StructMatrix(size, size);
        for(int i = 0; i < size; i++){
            _out[i, i] = 1;
        }
        return new LazyMatrix(_out, LazyMatrixOperation.Hold);
    }
    private static StructMatrix Hadamard_1Bit;
    private static LazyMatrix Hadamard_1Bit_Lazy;
    public static LazyMatrix Hadamard(int qbits) {
        if (qbits == 1){
            return Hadamard_1Bit_Lazy;
        }
        else {
            LazyMatrix _out = Hadamard_1Bit_Lazy;
            for(int i = 1; i < qbits; i++){
                _out = _out ^ Hadamard_1Bit;
            }
            return _out;
        }
    }


}
