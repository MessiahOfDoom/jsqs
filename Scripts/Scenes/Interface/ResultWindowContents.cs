using Godot;
using System;
using System.Collections.Generic;

public partial class ResultWindowContents : TabContainer
{
	
	private QCircuit lastCompiledCircuit = null;
	private int lastQBitCount = -1;

	public void setCircuit(QCircuit circuit, int qbits, string GraphJson) {
		lastCompiledCircuit = circuit;
		lastQBitCount = qbits;
		rerunMonteCarlo();
		setGraphPreview(GraphJson);
	}

	public void rerunMonteCarlo() {
		rerunMonteCarlo(10000);
	}
	public void rerunMonteCarlo(int runs) {
		var mc = lastCompiledCircuit.MonteCarlo(runs, 0);
		var graph = GetTree().Root.FindChild("MonteCarloGraph", recursive:true, owned:false) as MonteCarloGraph;
		graph.displayGraph(mc, runs, lastQBitCount);
	}

	public void setGraphPreview(string GraphJson) {
		var graph = GetTree().Root.FindChild("QuantumGraphPreview", recursive:true, owned:false) as QuantumGraphPreview;
		graph.LoadFromJsonString(GraphJson);
		lastCompiledCircuit.measurementGateGetter = graph.MeasurementByName;
		var dropdown = GetTree().Root.FindChild("CheckpointDropdown", recursive:true, owned: false) as OptionButton;
		dropdown.Clear();
		foreach(var node in graph.GetAllCheckpoints()) {
			var checkpoint = node as CheckpointGate;
			dropdown.AddItem(checkpoint.CheckpointName);
			dropdown.SetItemMetadata(dropdown.ItemCount - 1, checkpoint);
		}
		rerunAnalysisWindow();
	}

	public void rerunAnalysisWindow() {
		GD.Print("Rerunning the entire circuit: ");
		GD.Print(lastCompiledCircuit.RerunFromPart(0));
	}

	public void rerunAnalysisWindowFromCheckpoint() {
		var dropdown = GetTree().Root.FindChild("CheckpointDropdown", recursive:true, owned: false) as OptionButton;
		var checkpointIdx = 0;
		if(dropdown.Selected >= 0) {
			var checkpoint = dropdown.GetItemMetadata(dropdown.Selected).As<CheckpointGate>();
			if(checkpoint != null) {
				checkpointIdx = lastCompiledCircuit.CheckpointNameToPartIndex.GetValueOrDefault(checkpoint.CheckpointName, 0);
				GD.Print($"Rerunning the circuit from checkpoint '{checkpoint.CheckpointName}': " );
			}
		}
		
		if(checkpointIdx == 0) {
			GD.Print($"Rerunning the entire circuit: " );
		}
		GD.Print(lastCompiledCircuit.RerunFromPart(checkpointIdx));
	}

}
