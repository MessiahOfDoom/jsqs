using Godot;
using System;

[GlobalClass, Tool]
public partial class SlotHelper : Control
{
	
	private bool _left, _right;
	private Texture2D _leftTex, _rightTex;
	private int _leftType, _rightType;

	private int idx = 0;

	[Export]
	public bool Left {
		get => _left;
		set {
			_left = value;
			PropertiesChanged();
		}
	}

	[Export]
	public bool Right {
		get => _right;
		set {
			_right = value;
			PropertiesChanged();
		}
	}

	[Export]
	public Texture2D LeftTexture {
		get => _leftTex;
		set {
			_leftTex = value;
			PropertiesChanged();
		}
	}

	[Export]
	public Texture2D RightTexture {
		get => _rightTex;
		set {
			_rightTex = value;
			PropertiesChanged();
		}
	}

	[Export(PropertyHint.Enum, "QBit, Bit")]
	public int LeftType {
		get => _leftType;
		set {
			_leftType = value;
			PropertiesChanged();
		}
	}

	[Export(PropertyHint.Enum, "QBit, Bit")]
	public int RightType {
		get => _rightType;
		set {
			_rightType = value;
			PropertiesChanged();
		}
	}

	public override void _Ready()
	{
		PropertiesChanged();
	}

	public void PropertiesChanged() {
		GraphNode parent = GetParentOrNull<GraphNode>();
		if(parent == null) return;
		parent.SetSlotEnabledLeft(idx, Left);
		parent.SetSlotEnabledRight(idx, Right);
		if(Left)parent.SetSlotTypeLeft(idx, LeftType);
		if(Right)parent.SetSlotTypeRight(idx, RightType);
		if(Left)parent.SetSlotCustomIconLeft(idx, LeftTexture);
		if(Right)parent.SetSlotCustomIconRight(idx, RightTexture);
	}

	public SlotHelper() {

	}

	public SlotHelper(int index, bool l, bool r, int tyl, int tyr, Texture2D texl = null, Texture2D texr = null) {
		_left = l;
		_right = r;
		_leftType = tyl;
		_rightType = tyr;
		_leftTex = texl;
		_rightTex = texr;
		idx = index;
	}
	
}
