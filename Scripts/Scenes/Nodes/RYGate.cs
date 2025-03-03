using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class RYGate : GraphNode, ISaveableGate, ICompileableGate, IColorableGate
{

	private int _qbits = 1;
	[Export]
	public int QBits { 
		get => _qbits;
	 	set {
			_qbits = value;
			SetSlots();
		} 
	}

	private float Angle;

	// Called when the node enters the scene tree for the first time. 
	public override void _Ready()
	{
		Title = "RY Gate    ";
		SetSlots();
	}

	public void SetSlots() {
		foreach(var c in GetChildren()){
			if(!(c is Label) && !(c is SpinBox)) {
				RemoveChild(c);
				c.QueueFree();
			}
		}

		for(int i = 0; i < QBits; ++i) {
			var helper = new SlotHelper(i, true, true, 0, 0);
			helper.CustomMinimumSize = new Vector2(0, i == QBits - 1 ? 0 : 35);
			AddChild(helper);
		}

		var helper2 = new SlotHelper(QBits, false, false, 0, 0);
		helper2.CustomMinimumSize = new Vector2(0, 0);
		AddChild(helper2);

		this.Size = new Vector2(0, 0); //Autosize
	}

    public Dictionary<string, Variant> Save()
    {
        return new Dictionary<string, Variant>() {
			{"Filename", SceneFilePath},
			{"PosX", PositionOffset.X},
			{"PosY", PositionOffset.Y},
			{"Name", Name},
			{"Angle", Angle}
		};
    }

    public void Load(Dictionary<string, Variant> dict)
    {
        Angle = (float)dict["Angle"].AsDouble();
    }

    public LazyMatrix compile(int QBitCount, Array<int> ForQBits)
    {
        if(ForQBits.Count != 1) throw new ArgumentException("Compiling for an invalid number of QBits");
		var bit = ForQBits[0];
		if(bit == 0) return GateBuilder.Identity(QBitCount - 1) ^ GateBuilder.RY(Angle);
		else if(bit == QBitCount - 1) return GateBuilder.RY(Angle) ^ GateBuilder.Identity(QBitCount - 1);
		else return GateBuilder.Identity(QBitCount - bit - 1) ^ GateBuilder.RY(Angle) ^ GateBuilder.Identity(bit);
    }

	public void OnAngleChange(float angle) {
		this.Angle = angle;
	}

}
