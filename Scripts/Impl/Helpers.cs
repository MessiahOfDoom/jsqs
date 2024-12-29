using Godot;
using System;
using System.Globalization;
using System.IO;

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
		var CloseButtonScene = GD.Load<PackedScene>(ProjectSettings.GlobalizePath("res://Scenes/CloseButton.tscn"));
		Node closeButton = CloseButtonScene.Instantiate();
		if(closeButton is Button) {
			closeButton.Connect("pressed", Callable.From(() => {
				parent.RemoveNode(node);
				node.QueueFree();
			}));
		}
		node.GetTitlebarHBox().AddChild(closeButton);
	}

}
