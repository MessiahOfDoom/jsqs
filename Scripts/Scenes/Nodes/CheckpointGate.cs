using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class CheckpointGate : GraphNode, ISaveableGate, IResizeableGate, ICheckpointGate
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
		Title = "Checkpoint";
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
			{"PosX", Position.X},
			{"PosY", Position.Y},
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
