using Godot;
using System;

[GlobalClass, Tool]
public partial class HadamardGate : GraphNode
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
		Title = "Hadamard Gate";
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
