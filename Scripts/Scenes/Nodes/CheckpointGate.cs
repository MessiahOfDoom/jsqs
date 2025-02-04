using Godot;
using Godot.Collections;
using System;

[GlobalClass, Tool]
public partial class CheckpointGate : GraphNode, ISaveableGate, IResizeableGate, ICheckpointGate, IColorableGate
{

	public static Dictionary<string, CheckpointGate> checkpoints = new();

	private int _qbits = 1;
	[Export]
	public int QBits { 
		get => _qbits;
	 	set {
			_qbits = value;
			SetSlots();
		} 
	}

	public string CheckpointName = "";

	[Export]
	public LineEdit CheckpointNameEdit;

	public bool shouldRemove = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(!Engine.IsEditorHint()) {
			CheckpointNameEdit ??= FindChild("NameEdit", recursive:true, owned: false) as LineEdit;
			SetNextFreeName();
			if(GetParent() is QuantumGraphPreview) {
				CheckpointNameEdit.Editable = false;
				shouldRemove = false;
			}
		}
	}

    public override void _Notification(int what)
    {
        base._Notification(what);
		if(shouldRemove && what == NotificationPredelete && checkpoints.ContainsKey(CheckpointName)) {
			checkpoints.Remove(CheckpointName);
		}
    }

	public void SetNextFreeName(string name = "Checkpoint") {
		if(name == CheckpointName) return;
		if(GetParent() is QuantumGraphPreview) {
			CheckpointName = name;
		}
		else {
			if(checkpoints.ContainsKey(CheckpointName)) checkpoints.Remove(CheckpointName);
			CheckpointName = GetNextFreeCheckpointName(name);
			checkpoints.Add(CheckpointName, this);
		}
		Title = $"Checkpoint '{CheckpointName}'    ";
		if(CheckpointNameEdit != null) CheckpointNameEdit.Text = CheckpointName;
		Size = new(0,0); //Autosize
	}

	private string GetNextFreeCheckpointName(string name = "Checkpoint") {
		int i = -1;
		while(!isCheckpointNameFree(name + (++i == 0 ? "" : $" ({i})")));
		return name + (i == 0 ? "" : $" ({i})");
	}

	private static bool isCheckpointNameFree(string name) {
		GD.Print(!checkpoints.ContainsKey(name) + "   " + name);
		GD.Print(checkpoints);
		return !checkpoints.ContainsKey(name);
	}

    public void SetSlots() {
		foreach(var c in GetChildren()){
			if(!(c is LineEdit)) {
				RemoveChild(c);
				c.QueueFree();
			}
		}

		for(int i = 0; i < QBits; ++i) {
			var helper = new SlotHelper(i, true, true, 0, 0);
			helper.CustomMinimumSize = new Vector2(0, i == QBits - 1 ? 0 : 35);
			AddChild(helper);
		}

		var helper2 = new SlotHelper(QBits, false, false, 0, 0);
		helper2.CustomMinimumSize = new Vector2(0, 0);
		AddChild(helper2);

		this.Size = new Vector2(0, 0); //Autosize
	}

	public Dictionary<string, Variant> Save()
    {
        return new Dictionary<string, Variant>() {
			{"Filename", SceneFilePath},
			{"PosX", PositionOffset.X},
			{"PosY", PositionOffset.Y},
			{"Name", Name},
			{"CheckpointName", CheckpointName}
		};
    }

	public void Load(Dictionary<string, Variant> dict)
    {
        if(dict.ContainsKey("CheckpointName")) {
			SetNextFreeName(dict["CheckpointName"].AsStringName());
		}
		GD.Print(checkpoints);
    }

    public void SetSlotCount(int slotCount)
    {
        QBits = slotCount;
    }

	public int GetSlotCount()
    {
        return QBits;
    }

	public void OnNewNameSubmitted(string name) {
		SetNextFreeName(name);
	}

}
