using Godot;
using System;

public partial class ResultWindow : Window
{
	public void OnCloseRequest() {
		this.Visible = false;
	}
}
