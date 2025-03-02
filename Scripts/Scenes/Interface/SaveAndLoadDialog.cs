using Godot;
using System;

public partial class SaveAndLoadDialog : FileDialog
{
	private string lastDir = ProjectSettings.GlobalizePath("user://");

	private string jsonFilter = "*.json;JSON Files";
	
	private string jqcgFilter = "*.jqcg;JQCG Files";

	private bool jsonMode;

	[Signal]
	public delegate void SaveFileSelectedEventHandler(string filename);
	[Signal]
	public delegate void LoadFileSelectedEventHandler(string filename);

		[Signal]
	public delegate void SaveGateFileSelectedEventHandler(string filename);
	[Signal]
	public delegate void LoadGateFileSelectedEventHandler(string filename);

	public override void _Ready()
	{
		CurrentPath = lastDir;
		Filters[0] = jsonFilter;
		jsonMode = true;
	}

	public void setJsonMode(bool jMode) {
		var filters = Filters;
		filters[0] = jMode ? jsonFilter : jqcgFilter;
		Filters = filters;
		jsonMode = jMode;
	}

	public void OnSaveSignal() {
		setJsonMode(true);
		FileMode = FileModeEnum.SaveFile;
		Visible = true;
	}

	public void OnLoadSignal() {
		setJsonMode(true);
		FileMode = FileModeEnum.OpenFile;
		Visible = true;
	}

	public void OnSaveGateSignal() {
		setJsonMode(false);
		FileMode = FileModeEnum.SaveFile;
		Visible = true;
	}

	public void OnLoadGateSignal() {
		setJsonMode(false);
		FileMode = FileModeEnum.OpenFile;
		Visible = true;
	}

	public void OnFileSelected(string filename) {
		EmitSignal(FileMode == FileModeEnum.SaveFile ? (jsonMode ? SignalName.SaveFileSelected : SignalName.SaveGateFileSelected) : (jsonMode ? SignalName.LoadFileSelected : SignalName.LoadGateFileSelected), filename);
	}
}
