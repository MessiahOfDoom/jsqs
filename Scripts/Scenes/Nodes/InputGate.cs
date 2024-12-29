using Godot;
using System;

[GlobalClass, Tool]
public partial class InputGate : GraphNode
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

	[Export]
	private PackedScene ComplexInputSlotScene; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Title = "Input Gate";
		SetSlots();
	}

	public void SetSlots() {
		foreach(var c in GetChildren()){
			RemoveChild(c);
		}
		ComplexInputSlotScene ??= GD.Load<PackedScene>(ProjectSettings.GlobalizePath("res://Scenes/ComplexInputSlot.tscn"));
		for(int i = 0; i < QBits; ++i) {
			var slot = ComplexInputSlotScene.Instantiate();
			AddChild(slot);
			SetSlot(i, false, 0, new(), true, 0, new(1,1,1,1));
		}
		this.Size = new Vector2(0, 0); //Autosize
	}

	public void OnSetSlotCount(int count) {
		QBits = count;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
