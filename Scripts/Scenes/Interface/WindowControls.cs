using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

public partial class WindowControls : HBoxContainer
{

	[Export]
	public SideBar sideBar;

	[Export]
	public Control graph;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().Root.SizeChanged += OnWindowSizeChanged;
		DisplayServer.WindowSetMinSize(new(1280, 720));
	}

	public void OnWindowSizeChanged() {
		var windowSize = DisplayServer.WindowGetSize();
		sideBar.CustomMinimumSize = new(255, Math.Max(720, windowSize.Y));
		sideBar.UpdateSize();
	}
}
