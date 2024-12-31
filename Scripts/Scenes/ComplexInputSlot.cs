using Godot;
using System;

public partial class ComplexInputSlot : Control
{

	[Export]
	public LineEdit RealEdit;
	[Export]
	public LineEdit ComplexEdit;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public Complex GetValue() {
		double r = (double)Variant.From<string>(RealEdit.Text);
		double c = (double)Variant.From<string>(ComplexEdit.Text);
		return new(r,c);
	}

	public void SetValue(Complex value){
		RealEdit.Text = value.real.ToString();
		ComplexEdit.Text = value.imag.ToString();
	}
}
