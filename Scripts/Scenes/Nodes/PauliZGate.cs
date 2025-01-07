using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class PauliZGate : GraphNode, ISaveableGate, ICompileableGate
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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Title = "Pauli Z Gate    ";
		SetSlots();
	}

	public void SetSlots() {
		foreach(var c in GetChildren()){
			RemoveChild(c);
		}
		for(int i = 0; i < QBits; ++i) {
			var helper = new SlotHelper(true, true, 0, 0);
			helper.CustomMinimumSize = new Vector2(0, 35);
			AddChild(helper);
		}
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

    public LazyMatrix compile(int QBitCount, Array<int> ForQBits)
    {
        if(ForQBits.Count != 1) throw new ArgumentException("Compiling for an invalid number of QBits");
		var bit = ForQBits[0];
		if(bit == 0) return GateBuilder.PauliZ() ^ GateBuilder.Identity(QBitCount - 1);
		else if(bit == QBitCount - 1) return GateBuilder.Identity(QBitCount - 1) ^ GateBuilder.PauliZ();
		else return GateBuilder.Identity(bit) ^ GateBuilder.PauliZ() ^ GateBuilder.Identity(QBitCount - bit - 1);
    }

}
