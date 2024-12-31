using Godot;
using Godot.Collections;
using System;
using System.Linq;

[GlobalClass, Tool]
public partial class InputGate : GraphNode, ISaveableGate, IResizeableGate
{

	private int _qbits = 0;
	[Export]
	public int QBits { 
		get => _qbits;
	 	set {
			var values = GetQBitsForSaving();
			_qbits = value;
			SetSlots(values);
		} 
	}

	[Export]
	private PackedScene ComplexInputSlotScene; 

	[Signal]
	public delegate void SetQbitsFromInputGateEventHandler(int slots);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Title = "Input Gate";
		SetSlots(null);
	}

	public void SetSlots(Dictionary<int, double[]> values) {
		foreach(var c in GetChildren()){
			RemoveChild(c);
		}
		ComplexInputSlotScene ??= GD.Load<PackedScene>(ProjectSettings.GlobalizePath("res://Scenes/ComplexInputSlot.tscn"));
		for(int i = 0; i < QBits; ++i) {
			var slot = ComplexInputSlotScene.Instantiate();
			slot.Name = "slot" + i.ToString();
			AddChild(slot);
			SetSlot(i, false, 0, new(), true, 0, new(1,1,1,1));
			SetQBit(i, new(1));
		}
		if(values != null) {
			foreach(var pair in values) {
				if(pair.Key < QBits) {
					SetQBit(pair.Key, pair.Value);
				}
			}
		}
		this.Size = new Vector2(0, 0); //Autosize
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetQBit(int index, Complex value) {
		if (index < 0 || index >= QBits) throw new ArgumentOutOfRangeException("Index out of range");
		var input = FindChild("slot" + index.ToString(), owned: false);
		if (input == null || !(input is ComplexInputSlot complexInputSlot)) return;
		complexInputSlot.SetValue(value);
	}

	public Complex GetQBit(int index) {
		if (index < 0 || index >= QBits) throw new ArgumentOutOfRangeException("Index out of range");
		var input = FindChild("slot" + index.ToString(), owned:false);
		if (input == null || !(input is ComplexInputSlot complexInputSlot)) return 0;
		return complexInputSlot.GetValue();
	}

	public Dictionary<int, double[]> GetQBitsForSaving() {
		Dictionary<int, double[]> result = new();
		for(int i = 0; i < QBits; i++) {
			result[i] = GetQBit(i);
		}
		return result;
	}

    public Dictionary<string, Variant> Save()
    {
        return new Dictionary<string, Variant>() {
			{"Filename", SceneFilePath},
			{"PosX", Position.X},
			{"PosY", Position.Y},
			{"Name", Name},
			{"QBits", QBits},
			{"QBitValues", GetQBitsForSaving()}
		};
    }

    public void Load(Dictionary<string, Variant> dict)
    {

        EmitSignal(SignalName.SetQbitsFromInputGate, dict["QBits"]);
		var dict2 = dict["QBitValues"].AsGodotDictionary<int, double[]>();
		foreach(var pair in dict2) {
			SetQBit(pair.Key, pair.Value);
		}
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
