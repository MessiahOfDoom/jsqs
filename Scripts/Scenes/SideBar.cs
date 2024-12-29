using Godot;
using System;

[Tool]
public partial class SideBar : Node
{
    [Signal]
    public delegate void SetQBitCountEventHandler(int count);

    [Signal]
    public delegate void AddNodeEventHandler(PackedScene scene);

    [Export]
    public LineEdit QBitCountLineEdit;

    public override void _Ready()
    {
        OnQBitCountSliderChanged(1);
    }


    public void OnQBitCountSliderChanged(float value) {
        var val = Math.Floor(value);
        if (val <= 0 || val > 12) {
            throw new ArgumentOutOfRangeException($"Value {val} out of range: 1 <= value <= 16");
        } 
        if (QBitCountLineEdit != null) {
            QBitCountLineEdit.Text = val.ToString();
        }
        EmitSignal(SignalName.SetQBitCount, val);
    }

}
