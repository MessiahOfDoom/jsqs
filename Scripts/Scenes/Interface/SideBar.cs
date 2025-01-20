using Godot;
using System;

[Tool, GlobalClass]
public partial class SideBar : Control
{

    [Signal]
    public delegate void SetQBitCountEventHandler(int count);

    [Signal]
    public delegate void AddNodeEventHandler(PackedScene scene);

    [Signal]
    public delegate void SaveToJsonEventHandler();

    [Signal]
    public delegate void LoadFromJsonEventHandler();
    
    [Signal]
    public delegate void CompileAndRunEventHandler();

    [Export]
    public LineEdit QBitCountLineEdit;

    [Export]
    public HSlider QBitCountSlider;

    private int _maxQBits = 12; 

    [Export]
    public int MaxQBits {
        get => _maxQBits;
        set {
            _maxQBits = value;
            OnMaxQBitsChanged();
        }
    }

    public override void _Ready()
    {
        QBitCountLineEdit ??= FindChild("QBitCountText", recursive:true, owned: false) as LineEdit;
        QBitCountSlider ??= FindChild("QBitCountSlider", recursive:true, owned: false) as HSlider;
        OnQBitCountSliderChanged(1);
    }


    public void OnQBitCountSliderChanged(float value) {
        var val = Math.Floor(value);
        if (val <= 0 || val > MaxQBits) {
            throw new ArgumentOutOfRangeException($"Value {val} out of range: 1 ≤ value ≤ {MaxQBits}");
        } 
        if (QBitCountLineEdit != null) {
            QBitCountLineEdit.Text = val.ToString();
        }
        EmitSignal(SignalName.SetQBitCount, val);
    }

    public void OnQbitCountChangedExternal(int val) {
        QBitCountSlider.Value = val;
        if (val <= 0 || val > MaxQBits) {
            throw new ArgumentOutOfRangeException($"Value {val} out of range: 1 ≤ value ≤ {MaxQBits}");
        } 
        if (QBitCountLineEdit != null) {
            QBitCountLineEdit.Text = val.ToString();
        }
    }

    public void OnMaxQBitsChanged() {
        QBitCountSlider ??= FindChild("QBitCountSlider", recursive:true, owned: false) as HSlider;
        QBitCountSlider.MaxValue = MaxQBits;
    }

    public void SaveButtonPressed() {
        EmitSignal(SignalName.SaveToJson);
    }

    public void LoadButtonPressed() {
        EmitSignal(SignalName.LoadFromJson);
    }

    public void CompileButtonPressed() {
        EmitSignal(SignalName.CompileAndRun);
    }

    public void UpdateSize() {
        float height = CustomMinimumSize.Y;
        (FindChild("Settings", recursive:true, owned: false) as VBoxContainer).CustomMinimumSize = new(255, Math.Max(689, height - 31));
        (FindChild("Nodes", recursive:true, owned: false) as VBoxContainer).CustomMinimumSize = new(255, Math.Max(689, height - 31));
        (FindChild("Graphs", recursive:true, owned: false) as VBoxContainer).CustomMinimumSize = new(255, Math.Max(689, height - 31));
    }

}
