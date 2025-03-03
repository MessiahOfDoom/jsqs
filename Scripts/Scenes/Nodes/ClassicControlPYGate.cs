using Godot;
using Godot.Collections;
using System;
using System.Linq;

[GlobalClass, Tool]
public partial class ClassicControlPYGate : GraphNode, ISaveableGate, ICompileableGateWithBits, IColorableGate
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Title = "Controlled Pauli Y Gate    ";
		SetSlots();
	}

	public void SetSlots() {
		foreach(var c in GetChildren()){
			RemoveChild(c);
			c.QueueFree();
		}
		var helper = new SlotHelper(0, true, true, 0, 0);
		helper.CustomMinimumSize = new Vector2(0, 35);
		AddChild(helper);

        var helper2 = new SlotHelper(1, true, true, 1, 1);
        helper2.CustomMinimumSize = new Vector2(0, 35);
        AddChild(helper2);
		this.Size = new Vector2(0, 0); //Autosize
	}

    public Dictionary<string, Variant> Save()
    {
        return new Dictionary<string, Variant>() {
			{"Filename", SceneFilePath},
			{"PosX", PositionOffset.X},
			{"PosY", PositionOffset.Y},
			{"Name", Name}
		};
    }

    public void Load(Dictionary<string, Variant> dict)
    {
        //Nothing to do here
    }

    public LazyMatrix compile(int QBitCount, Array<int> ForQBits, Array<int> classicBits, ClassicControlBitMatrix.GetMatrixIndexByBits getter)
    {
        if(ForQBits.Count != 1) throw new ArgumentException("Compiling for an invalid number of QBits");
        var bit = ForQBits[0];
        var Matrix = new ClassicControlBitMatrix(new IMatrix[] {GateBuilder.Identity(1), GateBuilder.PauliY()}, classicBits.ToArray(), getter);
		if(bit == 0) return GateBuilder.Identity(QBitCount - 1) ^ Matrix;
		else if(bit == QBitCount - 1) return new LazyMatrix(Matrix, LazyMatrixOperation.Hold) ^ GateBuilder.Identity(QBitCount - 1);
		else return GateBuilder.Identity(QBitCount - bit - 1) ^ Matrix ^ GateBuilder.Identity(bit);
    }
}