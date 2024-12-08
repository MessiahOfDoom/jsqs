using Godot;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public partial class Main : GraphEdit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/*Matrix m = new Matrix(3, 2);
		m[0, 0] = 0;
		m[0, 1] = 1;
		m[1, 0] = 2;
		m[1, 1] = 3;
		m[2, 0] = 4;
		m[2, 1] = 5;
		GD.Print(m.ToString());
		GD.Print(m.Transposed().ToString());
		GD.Print(2 * GateBuilder.Identity(2));
		GD.Print(GateBuilder.Identity(1) ^ GateBuilder.Identity(1));

		Vector v = new Vector(4);
		v[0] = new Complex(0, 1);*/
		
		//GD.Print(Consts.SQRT_TWO_HALVES);
		//GD.Print(Consts.SQRT_TWO_HALVES * Consts.SQRT_TWO_HALVES * Consts.SQRT_TWO_HALVES * Consts.SQRT_TWO_HALVES);
		
		//GD.Print(v.ToString());
		//GD.Print((GateBuilder.Identity(1) ^ GateBuilder.Identity(1)) * v);
		//GD.Print(GateBuilder.Hadamard(10));
		var v = new Vector(16384*2);
		v[0] = 1;
		var stopwatch = Stopwatch.StartNew();
		var m =  GateBuilder.Identity(15);// ^ GateBuilder.Identity(2);
		stopwatch.Stop();
		GD.Print(stopwatch.Elapsed);
		GD.Print(m.getN());
		stopwatch.Restart();
		var v2 = m * v;
		stopwatch.Stop();
		GD.Print(stopwatch.Elapsed);
		GD.Print(v2);
	/*
		var path = ProjectSettings.GlobalizePath("user://test3.jsqs");
		m.WriteToFile(path);
		var m2 = StructMatrix.ReadFromFile(path);
		GD.Print(m == m2);*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
