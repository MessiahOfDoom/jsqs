using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class ToffoliGate : GraphNode, ISaveableGate, ICompileableGate, IMultiInputGate, IColorableGate
{

	private int _qbits = 3;
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
		Title = "Toffoli Gate    ";
		SetSlots();
	}

	public void SetSlots() {
		Sprite2D texture = null;
		foreach(var c in GetChildren()){
			if(!(c is Sprite2D)) {
				RemoveChild(c);
				c.QueueFree();
			}
			else texture = c as Sprite2D;
		}
		for(int i = 0; i < QBits; ++i) {
			var helper = new SlotHelper(i, true, true, 0, 0);
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
        if(ForQBits.Count != 3) throw new ArgumentException("Compiling for an invalid number of QBits");
		var matrix = GateBuilder.Toffoli();
		if(QBitCount > 3) matrix = matrix ^ GateBuilder.Identity(QBitCount - 3);
		return new LazyMatrix(matrix, LazyMatrixOperation.Shuffle, Helpers.QbitOrder(QBitCount, ForQBits));
    }

    public int GetSlotCount()
    {
        return QBits;
    }

}
