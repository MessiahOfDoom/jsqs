using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class QuantumGraphPreview : GraphEdit
{

	private List<GraphNode> nodes = new();

	[Export]
	public InputGate inputGate;

	[Export]
	public OutputGate outputGate;

	private int latestSlotCount = 1;

	private Color QBitColor = new(0xBE6EF5FF);
	private Color BitColor = new(0xA5F56EFF);
	public override void _Ready()
	{

		inputGate ??= FindChild("InputGate", recursive:true, owned:false) as InputGate;
		if(inputGate != null) {
			nodes.Add(inputGate);
			(inputGate as IColorableGate).SetSlotColors(QBitColor, BitColor);
		}

		outputGate ??= FindChild("OutputGate", recursive:true, owned:false) as OutputGate;
		if(outputGate != null) {
			nodes.Add(outputGate);
			(outputGate as IColorableGate).SetSlotColors(QBitColor, BitColor);
		}
	}
	

	public void AddGraphNode(GraphNode node) {
		nodes.Add(node);
		AddChild(node);
		if(node is IColorableGate gate) {
			gate.SetSlotColors(QBitColor, BitColor);
		}
	}

	public void RemoveNode(GraphNode node) {
		nodes.Remove(node);
		RemoveChild(node);
	}

	public void RemoveNodeByName(StringName name) {
		var node = nodes.Find(x => x.Name == name);
		RemoveNode(node);
	}

	public void AddGraphNodeFromScene(PackedScene scene) {
		Node node = scene.Instantiate();
		if (!(node is GraphNode)) throw new ArgumentException("Can't add Scene as new Node because it isn't a GraphNode");
		node.Name = Guid.NewGuid().ToString();
		GraphNode newNode = node as GraphNode;
		if(newNode is IResizeableGate res) res.SetSlotCount(latestSlotCount);
		AddGraphNode(node as GraphNode);
	} 

	private void ClearGraph() {
		ClearConnections();
		var nodesClone = new List<GraphNode>(nodes);
		foreach(var node in nodesClone) {
			if(node == inputGate || node == outputGate)continue;
			RemoveNode(node);
			if(node is CheckpointGate cg) cg.shouldRemove = false;
			node.QueueFree();
		}
	}

	public void LoadFromJsonString(string text) {
		ClearGraph();
		var json = new Json();
		var parseRes = json.Parse(text);
		if(parseRes != Error.Ok) {
			GD.PrintErr($"JSON Parse Error: {json.GetErrorMessage()} in '{text}' at line {json.GetErrorLine()}");
		}
		var data = new Godot.Collections.Dictionary<string, Variant>((Dictionary)json.Data);
		Array<Variant> gates = (Array<Variant>)data["gates"]; 
		foreach(Variant v in gates) {
			Godot.Collections.Dictionary<string, Variant> nodeData = (Godot.Collections.Dictionary<string, Variant>)v;
			GraphNode node;
			if(nodeData["Name"].AsString().Equals("InputGate", StringComparison.InvariantCultureIgnoreCase)) {
				node = inputGate;
			}else if(nodeData["Name"].AsString().Equals("OutputGate", StringComparison.InvariantCultureIgnoreCase)) {
				node = outputGate;
			}else {
				var scene = GD.Load<PackedScene>(nodeData["Filename"].AsString());
				node = scene.Instantiate<GraphNode>();
				node.Name = nodeData["Name"].AsString();
				AddGraphNode(node);
			}
			
			node.PositionOffset = new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]);
			if(node is ISaveableGate gate) gate.Load(nodeData);
		}

		Array<Dictionary> connections = (Array<Dictionary>)data["connections"];
		foreach(var conn in connections) {
			ConnectNode((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"]);
		}
		inputGate.EmitSignal(InputGate.SignalName.SetQbitsFromInputGate, latestSlotCount);
	}

	public void OnSetSlotCount(int slotCount){
		latestSlotCount = slotCount;
		foreach(var node in nodes) {
			if (node is IResizeableGate gate) {
				gate.SetSlotCount(slotCount);
			}
			if(node is IColorableGate col) {
				col.SetSlotColors(QBitColor, BitColor);
			}
		}
	}

	public GraphNode NodeByName(StringName name) {
		return nodes.Find(x => ((string)x.Name).Equals((string)name, StringComparison.InvariantCultureIgnoreCase));
	}

	public MeasurementGate MeasurementByName(StringName name) {
		return nodes.Find(x => ((string)x.Name).Equals((string)name, StringComparison.InvariantCultureIgnoreCase) && x is MeasurementGate) as MeasurementGate;
	}

	public CheckpointGate CheckpointByName(string name) {
		return nodes.Find(x => x is CheckpointGate checkpoint && checkpoint.CheckpointName == name) as CheckpointGate;
	}

	public List<GraphNode> GetAllCheckpoints() {
		return nodes.FindAll(x => x is CheckpointGate);
	}

}
