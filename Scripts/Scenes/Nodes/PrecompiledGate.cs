using Godot;
using Godot.Collections;
using System;
using System.Linq;

[GlobalClass, Tool]
public partial class PrecompiledGate : GraphNode, ISaveableGate, ICompileableGate, IColorableGate
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

	private string AsBase64String = "";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Title = "Precompiled Gate    ";
		SetSlots();
	}

	public void SetSlots() {
		foreach(var c in GetChildren()){
			RemoveChild(c);
			c.QueueFree();
		}
		for(int i = 0; i < QBits; ++i) {
			var helper = new SlotHelper(i, true, true, 0, 0);
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
			{"Name", Name},
			{"Base64", AsBase64String}
		};
    }

    public void Load(Dictionary<string, Variant> dict)
    {
        AsBase64String = dict["Base64"].AsString();
    }

    public LazyMatrix compile(int QBitCount, Array<int> ForQBits)
    {
        if(ForQBits.Count != 1) throw new ArgumentException("Compiling for an invalid number of QBits");
		var bit = ForQBits[0];
		if(bit == 0) return GateBuilder.Identity(QBitCount - 1) ^ GateBuilder.PauliX();
		else if(bit == QBitCount - 1) return GateBuilder.PauliX() ^ GateBuilder.Identity(QBitCount - 1);
		else return GateBuilder.Identity(QBitCount - bit - 1) ^ GateBuilder.PauliX() ^ GateBuilder.Identity(bit);
    }

	public void InitializeFromFile(string filename) {
		GD.Print(filename.Replace("\\", "/"));
		var name = filename.Replace("\\", "/").Split("/").Last().Replace(".jqcg", "");
		Title = $"Precompiled Gate '{name}'    ";
		using var file = FileAccess.Open(filename, FileAccess.ModeFlags.Read);
		AsBase64String = file.GetAsText();
		var mat = LazyMatrix.FromByteArray(Convert.FromBase64String(AsBase64String));
		var N = mat.getN();
		var M = mat.getM();
		if(M != N) throw new Exception("Matrix isn't square, couldn't build a Gate out of it.");
		QBits = (int)Math.Ceiling(Math.Log2(N));
		if((1 << QBits) != N) throw new Exception("Matrix dimensions aren't a multiple of 2, couldn't build a Gate out of it.");
		//TODO maybe check for unitary matrix
	}

}
