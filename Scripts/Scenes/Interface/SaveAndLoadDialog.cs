using Godot;
using System;

public partial class SaveAndLoadDialog : FileDialog
{
	private string lastDir = ProjectSettings.GlobalizePath("user://");

	[Signal]
	public delegate void SaveFileSelectedEventHandler(string filename);
	[Signal]
	public delegate void LoadFileSelectedEventHandler(string filename);

	public override void _Ready()
	{
		CurrentPath = lastDir;
	}

	public void OnSaveSignal() {

		FileMode = FileModeEnum.SaveFile;
		Visible = true;
	}

	public void OnLoadSignal() {
		FileMode = FileModeEnum.OpenFile;
		Visible = true;
	}

	public void OnFileSelected(string filename) {
		EmitSignal(FileMode == FileModeEnum.SaveFile ? SignalName.SaveFileSelected : SignalName.LoadFileSelected, filename);
	}
}
