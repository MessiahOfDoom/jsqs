using Godot;
using System;

[GlobalClass]
public partial class ComplexInputSlot : Control
{

	[Export]
	public LineEdit RealEditZero;
	[Export]
	public LineEdit ComplexEditZero;

	[Export]
	public LineEdit RealEditOne;
	[Export]
	public LineEdit ComplexEditOne;

	[Export]
	public TextureRect ValidityCheck;

	[Export]
	public Texture2D ValidTexture;

	[Export]
	public Texture2D InvalidTexture;

	public Vector GetValue() {
		double rz = Variant.From<string>(RealEditZero.Text).AsDouble();
		double cz = Variant.From<string>(ComplexEditZero.Text).AsDouble();
		double ro = Variant.From<string>(RealEditOne.Text).AsDouble();
		double co = Variant.From<string>(ComplexEditOne.Text).AsDouble();
		Vector _out = new(2);
		_out[0] = new(rz, cz);
		_out[1] = new(ro, co);
		return _out;
	}

	public void SetValue(Vector value){
		if(value.length < 2) return;
		RealEditZero.Text = Variant.From<double>(value[0].real).AsString();
		ComplexEditZero.Text = Variant.From<double>(value[0].imag).AsString();
		RealEditOne.Text = Variant.From<double>(value[1].real).AsString();
		ComplexEditOne.Text = Variant.From<double>(value[1].imag).AsString();
		OnValueChanged();
	}

	public void OnValueChanged() {
		ValidityCheck.Texture = QBitValid() ? ValidTexture : InvalidTexture;
	}

	public bool QBitValid() {
		return (Math.Abs(1 - GetValue().Abs2()) < 5e-5);
	}

	public void SetEnabled(bool enabled) {
		RealEditZero.Editable = enabled;
		RealEditOne.Editable = enabled;
		ComplexEditZero.Editable = enabled;
		ComplexEditOne.Editable = enabled;
	}
}
