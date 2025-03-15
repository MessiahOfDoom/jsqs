using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Text;

[GlobalClass, Tool]
public partial class PrecompiledGate : GraphNode, ISaveableGate, ICompileableGate, IColorableGate, IMultiInputGate
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
	private string CircuitPreviewBase64 = "";
	private string gateName = "";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Title = $"Precompiled Gate '{gateName}'    ";
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
			{"Base64", AsBase64String},
			{"PreviewBase64", CircuitPreviewBase64},
			{"GateName", gateName}
		};
    }

    public void Load(Dictionary<string, Variant> dict)
    {
        AsBase64String = dict["Base64"].AsString();
		var mat = LazyMatrix.FromByteArray(Convert.FromBase64String(AsBase64String));
		var N = mat.getN();
		var M = mat.getM();
		if(M != N) throw new Exception("Matrix isn't square, couldn't build a Gate out of it.");
		QBits = (int)Math.Ceiling(Math.Log2(N));
		if((1 << QBits) != N) throw new Exception("Matrix dimensions aren't a multiple of 2, couldn't build a Gate out of it.");
		gateName = dict["GateName"].AsString();
		CircuitPreviewBase64 = dict["PreviewBase64"].AsString();
    }

    public LazyMatrix compile(int QBitCount, Array<int> ForQBits)
    {
        if(ForQBits.Count != QBits) throw new ArgumentException("Compiling for an invalid number of QBits");
		var matrix = LazyMatrix.FromByteArray(Convert.FromBase64String(AsBase64String));
		if(QBitCount > QBits) matrix = matrix ^ GateBuilder.Identity(QBitCount - QBits);
		return new LazyMatrix(matrix, LazyMatrixOperation.Shuffle, Helpers.QbitOrder(QBitCount, ForQBits));
    }

	public void InitializeFromFile(string filename) {
		gateName = filename.Replace("\\", "/").Split("/").Last().Replace(".jqcg", "");
		Title = $"Precompiled Gate '{gateName}'    ";
		using var file = FileAccess.Open(filename, FileAccess.ModeFlags.Read);
		AsBase64String = file.GetLine();
		var mat = LazyMatrix.FromByteArray(Convert.FromBase64String(AsBase64String));
		var N = mat.getN();
		var M = mat.getM();
		if(M != N) throw new Exception("Matrix isn't square, couldn't build a Gate out of it.");
		QBits = (int)Math.Ceiling(Math.Log2(N));
		if((1 << QBits) != N) throw new Exception("Matrix dimensions aren't a multiple of 2, couldn't build a Gate out of it.");
		CircuitPreviewBase64 = file.GetLine();
	}

    public int GetSlotCount()
    {
        return QBits;
    }

	public void OnGuiInput(InputEvent ev) {
		if(ev is InputEventMouseButton mb) {
			if(mb.ButtonIndex == MouseButton.Left && mb.DoubleClick) {
				var preview = GetTree().Root.FindChild("CompiledPreview", recursive:true, owned:false) as CompiledPreview;
				if(preview != null) {
					preview.ShowCircuit(CircuitPreviewBase64);
				}
			}
		}
	} 
}
