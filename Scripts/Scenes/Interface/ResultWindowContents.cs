using Godot;
using System;
using System.Collections.Generic;

public partial class ResultWindowContents : TabContainer
{
	
	private QCircuit lastCompiledCircuit = null;
	private int lastQBitCount = -1;
	private bool initialized = false;

	public void setCircuit(QCircuit circuit, int qbits, string GraphJson) {
		GD.Print(GraphJson);
		lastCompiledCircuit = circuit;
		lastQBitCount = qbits;
		rerunMonteCarlo();
		setGraphPreview(GraphJson);
		setupStatePreview();
		initialized = true;
		SelectStateCheckpoint();
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
		var _out = lastCompiledCircuit.RerunFromPart(0);
		GD.Print(_out);
		if(initialized) SelectStateCheckpoint();
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
		var _out = lastCompiledCircuit.RerunFromPart(checkpointIdx);
		GD.Print(_out);
		if(initialized) SelectStateCheckpoint();
	}

	public void ClearStateRichText() {
		var label = GetTree().Root.FindChild("StateRichTextLabel", recursive: true, owned: false) as RichTextLabel;
		label.Text = "";
	}
	public void SetStateRichText(Vector v) {
		var label = GetTree().Root.FindChild("StateRichTextLabel", recursive: true, owned: false) as RichTextLabel;
		label.Text = v.ToBBCode();
	}

	public void AppendStateRichText(Vector v) {
		var label = GetTree().Root.FindChild("StateRichTextLabel", recursive: true, owned: false) as RichTextLabel;
		label.Text += $"{v.ToBBCode()}\n";
	}

	public void setupStatePreview() {
		var dropdown = GetTree().Root.FindChild("StateCheckpointDropdown", recursive:true, owned: false) as OptionButton;
		var graph = GetTree().Root.FindChild("QuantumGraphPreview", recursive:true, owned:false) as QuantumGraphPreview;
		dropdown.Clear();
		dropdown.AddItem("Input");
		dropdown.SetItemMetadata(dropdown.ItemCount - 1, graph.inputGate);
		foreach(var node in graph.GetAllCheckpoints()) {
			var checkpoint = node as CheckpointGate;
			dropdown.AddItem(checkpoint.CheckpointName);
			dropdown.SetItemMetadata(dropdown.ItemCount - 1, checkpoint);
		}
		dropdown.AddItem("Output");
		dropdown.SetItemMetadata(dropdown.ItemCount - 1, graph.outputGate);
		dropdown.Select(dropdown.ItemCount - 1);
	}

	public void SelectStateMode(int idx) {
		GD.Print(idx);
		if(idx == 0) {
			showStateAtCheckpoint();
		}
		else if(idx == 1) {
			showAllStatesFromCheckpoint();
		}
	}

	public void SelectStateCheckpoint() {
		var dropdown = GetTree().Root.FindChild("StateModeDropdown", recursive:true, owned: false) as OptionButton;
		if(dropdown.Selected >= 0) {
			SelectStateMode(dropdown.Selected);
		}
	}

	public void showStateAtCheckpoint() {
		var dropdown = GetTree().Root.FindChild("StateCheckpointDropdown", recursive:true, owned: false) as OptionButton;
		var checkpointIdx = -1;
		if(dropdown.Selected >= 0) {
			var checkpoint = dropdown.GetItemMetadata(dropdown.Selected).As<GraphNode>();
			if(checkpoint != null) {
				if(checkpoint is CheckpointGate checkpointGate) {
					checkpointIdx = lastCompiledCircuit.CheckpointNameToPartIndex.GetValueOrDefault(checkpointGate.CheckpointName, 0);
					GD.Print($"Rerunning the circuit from checkpoint '{checkpointGate.CheckpointName}': " );
				}
				else if (checkpoint is InputGate) {
					checkpointIdx = 0;
				}else if (checkpoint is OutputGate) {
					checkpointIdx = lastCompiledCircuit.parts.Count;
				}
			}
		}
		
		if(checkpointIdx == -1) {
			dropdown.Selected = 0;
			return;
		}
		var _out = lastCompiledCircuit.inputs[checkpointIdx];
		SetStateRichText(_out);
	}

	public void showAllStatesFromCheckpoint() {
		var dropdown = GetTree().Root.FindChild("StateCheckpointDropdown", recursive:true, owned: false) as OptionButton;
		var checkpointIdx = -1;
		if(dropdown.Selected >= 0) {
			var checkpoint = dropdown.GetItemMetadata(dropdown.Selected).As<GraphNode>();
			if(checkpoint != null) {
				if(checkpoint is CheckpointGate checkpointGate) {
					checkpointIdx = lastCompiledCircuit.CheckpointNameToPartIndex.GetValueOrDefault(checkpointGate.CheckpointName, 0);
					GD.Print($"Rerunning the circuit from checkpoint '{checkpointGate.CheckpointName}': " );
				}
				else if (checkpoint is InputGate) {
					checkpointIdx = 0;
				}else if (checkpoint is OutputGate) {
					checkpointIdx = lastCompiledCircuit.parts.Count;
				}
			}
		}
		
		if(checkpointIdx == -1) {
			dropdown.Selected = 0;
			return;
		}

		ClearStateRichText();
		foreach(var v in lastCompiledCircuit.GetAllPossibleStatesFromPart(checkpointIdx)) {
			AppendStateRichText(v);
		}
		
	}
}
