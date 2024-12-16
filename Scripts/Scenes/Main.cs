using Godot;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public partial class Main : GraphEdit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var v = new Vector(256);
		v[0] = 1;
		var stopwatch = Stopwatch.StartNew();
		var m = GateBuilder.Identity(8) * GateBuilder.Hadamard(8) * GateBuilder.Identity(8);
		stopwatch.Stop();
		GD.Print(stopwatch.Elapsed);
		GD.Print(m.getN());
		stopwatch.Restart();
		var v2 = m * v;
		stopwatch.Stop();
		GD.Print(stopwatch.Elapsed);
		m.EvalCache();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
