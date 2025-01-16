using Godot;
using System;

public partial class ErrorDialog : AcceptDialog
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Title = "Error compiling the graph.";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ShowError(string message) {
		DialogText = message;
		Show();
	}
}
