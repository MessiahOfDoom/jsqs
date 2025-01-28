using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class OutputGate : GraphNode, ISaveableGate, IResizeableGate, ICheckpointGate, IColorableGate
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
		Title = "Output    ";
		SetSlots();
	}

	public void SetSlots() {
		foreach(var c in GetChildren()){
			RemoveChild(c);
			c.QueueFree();
		}
		for(int i = 0; i < QBits; ++i) {
			var helper = new SlotHelper(i, true, false, 0, 0);
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

    public void SetSlotCount(int slotCount)
    {
        QBits = slotCount;
    }

	public int GetSlotCount()
    {
        return QBits;
    }

}
