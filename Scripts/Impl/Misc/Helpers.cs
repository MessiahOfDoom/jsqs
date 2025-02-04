using Godot;
using Godot.Collections;
using System;
using System.Linq;

public record Helpers {
	public static double[] DoublesFromBytes(byte[] bytes) {
		var _out = new double[bytes.Length / sizeof(double)];
		Buffer.BlockCopy(bytes, 0, _out, 0, bytes.Length);
		return _out;
	}

	public static byte[] BytesFromDoubles(double[] doubles) {
		var _out = new byte[doubles.Length * sizeof(double)];
		Buffer.BlockCopy(doubles, 0, _out, 0, _out.Length);
		return _out;
	}

	public static byte[] BytesFromInt(int i) {
		var _out = new byte[4];
		for(int j = 0; j < 4; j++){
			_out[j] = (byte)(i >> (8 * j) & 0xff);
		}
		return _out;
	}

	public static int IntFromBytes(byte[] bytes) {
		return bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0];
	}
	public static byte[] GetByteSubArray(byte[] full, int offset, int length) {
		byte[] _out = new byte[length];
		Buffer.BlockCopy(full, offset, _out, 0, length);
		return _out;
	}

	public static byte[] Append(byte[] b1, byte[] b2) {
		byte[] _out = new byte[b1.Length + b2.Length];
		Buffer.BlockCopy(b1, 0, _out, 0, b1.Length);
		Buffer.BlockCopy(b2, 0, _out, b1.Length, b2.Length);
		return _out;
	}

	public static T[] ArrayWithValue<T>(int size, Func<T> factory) {
		T[] _out = new T[size];
		for(int i = 0; i < size; ++i) {
			_out[i] = factory();
		}
		return _out;
	}

	public static void AddCloseButton(GraphNode node, QuantumGraph parent) {
		var CloseButtonScene = GD.Load<PackedScene>(ProjectSettings.GlobalizePath("res://Scenes/Misc/CloseButton.tscn"));
		Node closeButton = CloseButtonScene.Instantiate();
		if(closeButton is Button) {
			closeButton.Connect("pressed", Callable.From(() => {
				parent.RemoveNode(node);
				node.QueueFree();
			}));
		}
		node.GetTitlebarHBox().AddChild(closeButton);
	}

	public static Vector ZeroQBit() {
		Vector _out = new(2);
		_out[0] = 1;
		return _out;
	}

	public static Vector OneQBit() {
		Vector _out = new(2);
		_out[1] = 1;
		return _out;
	}

	public static void ShowErrorOnNode(GraphNode node) {
		var stylebox = node.GetThemeStylebox("titlebar");
		var styleboxSel = node.GetThemeStylebox("titlebar_selected");
		if((stylebox is StyleBoxFlat flat) && (styleboxSel is StyleBoxFlat flatSel)) {
			var color = flat.BgColor;
			var colorSel = flatSel.BgColor;
			var red = new Color(1, 0, 0);
			var flatNew = flat.Duplicate() as StyleBoxFlat;
			var flatSelNew = flatSel.Duplicate() as StyleBoxFlat;
			node.AddThemeStyleboxOverride("titlebar", flatNew);
			node.AddThemeStyleboxOverride("titlebar_selected", flatSelNew);
			var tween = node.CreateTween().SetLoops(5);
			flatNew.BgColor = red;
			flatSelNew.BgColor = red;
			tween.TweenCallback(Callable.From(() => {
				flatNew.BgColor = color;
				flatSelNew.BgColor = colorSel;
			})).SetDelay(1);
			tween.TweenCallback(Callable.From(() => {
				flatNew.BgColor = red;
				flatSelNew.BgColor = red;
			})).SetDelay(1);
			tween.Finished += () => {
				node.RemoveThemeStyleboxOverride("titlebar");
				node.RemoveThemeStyleboxOverride("titlebar_selected");
				tween.Kill();
			};
		}
	}

	public static void ShowErrorOnPort(GraphNode node, int port, bool left = false) {
		var slotColor = node.GetSlotColorRight(port);
		var red = new Color(1, 0, 0);
		var tween = node.CreateTween().SetLoops(5);
		
		if(left) node.SetSlotColorLeft(port, red);
		else node.SetSlotColorRight(port, red);
		tween.TweenCallback(Callable.From(() => {
			if(left) node.SetSlotColorLeft(port, slotColor);
			else node.SetSlotColorRight(port, slotColor);
		})).SetDelay(1);
		tween.TweenCallback(Callable.From(() => {
			if(left) node.SetSlotColorLeft(port, red);
			else node.SetSlotColorRight(port, red);
		})).SetDelay(1);
		tween.Finished += () => {
			if(left) node.SetSlotColorLeft(port, slotColor);
			else node.SetSlotColorRight(port, slotColor);
			tween.Kill();
		};
	}

	public static int[] QbitOrder(int QBitCount, Array<int> ForQBits){
		int[] _out = new int[QBitCount];
		for(int i = 0; i < ForQBits.Count; ++i) {
			_out[QBitCount - i - 1] = ForQBits[ForQBits.Count - i - 1];
		}
		var idx = ForQBits.Count;
		for(int i = 0; i < QBitCount; ++i){
			if(!_out.Contains(i)) {
				_out[QBitCount - idx - 1] = i;
				++idx;
			}
		}
		return _out;
	}

	public static WeightedRandom<int> WeightedRandomFromVector(Vector v) {
        WeightedRandom<int> _out = new();
		if(v == null) return _out;
        for(int i = 0; i < v.length; ++i) {
            _out.AddItem(i, v[i].Abs2());
        } 
        return _out;
    }

	public static WeightedRandom<int> WeightedRandomFromProbabilityVector(Vector v) {
        WeightedRandom<int> _out = new();
		if(v == null) return _out;
        for(int i = 0; i < v.length; ++i) {
            _out.AddItem(i, v[i].Abs());
        } 
        return _out;
    }

}
