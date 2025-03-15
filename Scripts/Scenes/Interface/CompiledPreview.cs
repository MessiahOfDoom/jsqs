using Godot;
using System;
using System.Text;

public partial class CompiledPreview : Window
{
	public void ShowCircuit(string Base64) {
		try {
			string json = Encoding.UTF8.GetString(Convert.FromBase64String(Base64));
			var graph = GetTree().Root.FindChild("QuantumGraphPreviewCompiled", recursive:true, owned:false) as QuantumGraphPreview;
			graph.LoadFromJsonString(json);
			graph.inputGate.SetEnabled(false);
			Show();
		}catch (Exception ex) {
			var parent = GetTree().Root.FindChild("GraphEdit", recursive:true, owned:false) as QuantumGraph;
			parent.EmitSignal(QuantumGraph.SignalName.CircuitCompileError, ex.Message);
		}
	}
}
