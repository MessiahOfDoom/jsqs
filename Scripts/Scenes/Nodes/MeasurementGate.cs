using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class MeasurementGate : GraphNode, ISaveableGate, IColorableGate
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
    public Texture2D Measurement0Texture;
    [Export]
    public Texture2D Measurement1Texture;
    
    public int LastMeasurement = -1; 

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Title = "Measurement    ";
		SetSlots();
        SetMeasurement(LastMeasurement);
	}

	public void SetSlots() {
		foreach(var c in GetChildren()){
			if(!(c is Sprite2D)) {
                RemoveChild(c);
                c.QueueFree();
            }
		}
		for(int i = 0; i < QBits; ++i) {
			var helper = new SlotHelper(i, true, true, 0, 0);
			helper.CustomMinimumSize = new Vector2(0, 35);
			AddChild(helper);
		}
        for(int i = 0; i < QBits; ++i) {
			var helper = new SlotHelper(i + QBits, false, true, 0, 1);
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

    public void SetMeasurement(int measurement) {
        LastMeasurement = measurement;
        var sprite = FindChild("Texture", recursive:true, owned: false) as Sprite2D;
        if(sprite == null) return;
        if(LastMeasurement == 0) sprite.Texture = Measurement0Texture;
        else if(LastMeasurement == 1) sprite.Texture = Measurement1Texture;
        else sprite.Texture = null;
    }

}