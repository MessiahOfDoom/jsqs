using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

public partial class QuantumGraph : GraphEdit
{

	private List<GraphNode> nodes = new();

	//TODO implement a way to keep track of node order in the tree to a) prevent loops, and b) make sure the circuit stays sane and compilable 
	private System.Collections.Generic.Dictionary<StringName, HashSet<StringName>> tree = new(); 
	// Called when the node enters the scene tree for the first time.

	[Export]
	private InputGate inputGate;

	[Export]
	private OutputGate outputGate;

	[Signal]
	public delegate void CircuitCompileErrorEventHandler(string description);

	private int latestSlotCount = 1;

	private Color QBitColor = new(0xBE6EF5FF);
	private Color BitColor = new(0xA5F56EFF);

	private bool QBitOrderAscending = false;

	private Compiler compiler;
	public override void _Ready()
	{

		inputGate ??= FindChild("InputGate", recursive:true, owned:false) as InputGate;
		if(inputGate != null) {
			nodes.Add(inputGate);
			tree.Add(inputGate.Name, new());
			(inputGate as IColorableGate).SetSlotColors(QBitColor, BitColor);
		}

		outputGate ??= FindChild("OutputGate", recursive:true, owned:false) as OutputGate;
		if(outputGate != null) {
			nodes.Add(outputGate);
			tree.Add(outputGate.Name, new());
			(outputGate as IColorableGate).SetSlotColors(QBitColor, BitColor);
		}

		compiler = new Compiler(NodeByName, (name, port, from) => GetConnection(name, port, from), GetInputOrThrow);
	}
	public void OnConnectionRequest(string fromNode, int fromPort, string toNode, int toPort) {
		var conn = GetConnection(fromNode, fromPort, true);
		if(conn != null) DisconnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"], false);
		conn = GetConnection(toNode, toPort, false);
		if(conn != null) DisconnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"]);
		if(!WouldContainLoop(fromNode, fromPort, toNode, toPort)) ConnectNodeInternal(fromNode, fromPort, toNode, toPort);
	}

	public void OnDisconnectionRequest(string fromNode, int fromPort, string toNode, int toPort) {
		DisconnectNodeInternal(fromNode, fromPort, toNode, toPort);
	}

	public void OnConnectionToFromEmpty(string node, int port, Vector2 pos, bool from) {
		var conn = GetConnection(node, port, from);
		if(conn != null) DisconnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"]);
	}

	private Dictionary GetConnection(string node, int port, bool from) {
		var connections = GetConnectionList();
		if(from) {
			foreach(var conn in connections) {
				if(((StringName)conn["from_node"]) == node && ((int)conn["from_port"]) == port) {
					return conn;
				}
			}
		}
		else {
			foreach(var conn in connections) {
				if(((StringName)conn["to_node"]) == node && ((int)conn["to_port"]) == port) {
					return conn;
				}
			}
		}
		return null;
	}

	public void DisconnectNodeInternal(StringName fromNode, int fromPort, StringName toNode, int toPort, bool rebuildTree = true) {
		DisconnectNode(fromNode, fromPort, toNode, toPort);
		if(rebuildTree) {
			RebuildTree();
		}
	}

	public void ConnectNodeInternal(StringName fromNode, int fromPort, StringName toNode, int toPort) {
		ConnectNode(fromNode, fromPort, toNode, toPort);
		tree[fromNode].Add(toNode);
		tree[fromNode].UnionWith(tree[toNode]);
	}

	public bool WouldContainLoop(string fromNode, int fromPort, string toNode, int toPort) {
		if(fromNode == toNode) return true;
		if(tree[toNode].Contains(fromNode)) return true;
		return false;
	}

	public void AddGraphNode(GraphNode node) {
		nodes.Add(node);
		tree.Add(node.Name, new());
		AddChild(node);
		if(node is IColorableGate gate) {
			gate.SetSlotColors(QBitColor, BitColor);
		}
	}

	public void RemoveNode(GraphNode node) {
		nodes.Remove(node);
		var connections = GetConnectionList();
		foreach (var conn in connections) {
			if((StringName)conn["from_node"] == node.Name || (StringName)conn["to_node"] == node.Name) {
				DisconnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"], false);
			}
		}
		RemoveChild(node);
		RebuildTree();
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
		Helpers.AddCloseButton(newNode, this);
		if(newNode is IResizeableGate res) res.SetSlotCount(latestSlotCount);
		AddGraphNode(node as GraphNode);
	} 

	private void RebuildTree(){
		tree.Clear();
		foreach(GraphNode node in nodes) {
			tree.Add(node.Name, new());
		}
		foreach(var conn in GetConnectionList()) {
			tree[(StringName)conn["from_node"]].Add((StringName)conn["to_node"]);
		}
		foreach(GraphNode node in nodes) {
			RebuildTreeRecursive(node.Name);
		}
	}

	private void RebuildTreeRecursive(StringName currentNode) {
		HashSet<StringName> set = new();
		foreach(StringName s in tree[currentNode]) {
			RebuildTreeRecursive(s);
			set.UnionWith(tree[s]);
		}
		tree[currentNode].UnionWith(set);
	}

	public void SaveToJson(string filename) {
		using var Savefile = FileAccess.Open(filename, FileAccess.ModeFlags.Write);
		Savefile.StoreString(AsJsonString());
	}

	public string AsJsonString() {
		Godot.Collections.Dictionary<string, Variant> graph = new();
		Array<Variant> gates = new();
		foreach(GraphNode node in nodes) {
			if(node is ISaveableGate gate) {
				gates.Add(gate.Save());
			}
		}
		graph.Add("gates", gates);
		graph.Add("connections", GetConnectionList());
		return Json.Stringify(graph, indent: "\t");
	}

	public void LoadFromJson(string filename) {
		
		using var Savefile = FileAccess.Open(filename, FileAccess.ModeFlags.Read);
		string text = Savefile.GetAsText();
		LoadFromJsonString(text);
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
		CheckpointGate.checkpoints.Clear();
	}

	public void LoadFromJsonString(string text) {
		ClearGraph();
		var json = new Json();
		var parseRes = json.Parse(text);
		if(parseRes != Error.Ok) {
			GD.PrintErr($"JSON Parse Error: {json.GetErrorMessage()} in '{text}' at line {json.GetErrorLine()}");
			EmitSignal(SignalName.CircuitCompileError, $"JSON Parse Error: {json.GetErrorMessage()} in '{text}' at line {json.GetErrorLine()}");
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
				Helpers.AddCloseButton(node, this);
				AddGraphNode(node);
			}
			
			node.PositionOffset = new Vector2((float)nodeData["PosX"], (float)nodeData["PosY"]);
			if(node is ISaveableGate gate) gate.Load(nodeData);
		}

		Array<Dictionary> connections = (Array<Dictionary>)data["connections"];
		foreach(var conn in connections) {
			ConnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"]);
		}
		inputGate.EmitSignal(InputGate.SignalName.SetQbitsFromInputGate, latestSlotCount);
	}

	public void OnSetSlotCount(int slotCount){
		var connections = GetConnectionList();
		foreach(var conn in connections) {
			if((int)conn["from_port"] >= slotCount) {
				var NodeFrom = NodeByName((StringName)conn["from_node"]);
				if (NodeFrom != null && NodeFrom is IResizeableGate) {
					DisconnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"], false);
					continue;
				}
			}
			if((int)conn["to_port"] >= slotCount) {
				var NodeTo = NodeByName((StringName)conn["to_node"]);
				if (NodeTo != null && NodeTo is IResizeableGate) {
					DisconnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"], false);
					continue;
				}
			}
		}
		latestSlotCount = slotCount;
		foreach(var node in nodes) {
			if (node is IResizeableGate gate) {
				gate.SetSlotCount(slotCount);
			}
			if(node is IColorableGate col) {
				col.SetSlotColors(QBitColor, BitColor);
			}
		}
		RebuildTree();
	}

	private GraphNode NodeByName(StringName name) {
		return nodes.Find(x => ((string)x.Name).Equals((string)name, StringComparison.InvariantCultureIgnoreCase));
	}

	public QCircuit Compile() {
		return compiler.compile(latestSlotCount, inputGate, QBitOrderAscending);
	}

	public Vector GetInputOrThrow() {
		if(!inputGate.AllQBitsValid()) {
			EmitSignal(SignalName.CircuitCompileError, "Not all QBits are valid, cannot run.");
			throw new Exception();
		}
		return inputGate.GetInput(QBitOrderAscending);
	}

	public void CompileAndRun() {
		if(!inputGate.AllQBitsValid()) {
			EmitSignal(SignalName.CircuitCompileError, "Not all QBits are valid, cannot run.");
			return;
		}
		try {
			QCircuit c = Compile();
			Vector input = inputGate.GetInput(QBitOrderAscending);
			GD.Print("Running Graph with input: " + input);
			var output = c.Run();
			GD.Print("Got output: " + output);

			GD.Print("Probabilities: " + c.CalculateProbabilityVector(input));
			
			var ResultWindow = GetTree().Root.FindChild("ResultWindow", recursive:true, owned:false) as Window;
			ResultWindow.Visible = true;
			var resultWindowContents = GetTree().Root.FindChild("ResultWindowContents", recursive:true, owned:false) as ResultWindowContents;
			resultWindowContents.setCircuit(c, latestSlotCount, AsJsonString());
		} catch (Exception ex) {
			EmitSignal(SignalName.CircuitCompileError, ex.Message);
			GD.PrintErr(ex);
		}
	}

	public void SetQBitOrderAscending(bool ascending) {
		QBitOrderAscending = ascending;
	}

	public void CompileToGate(string filename) {
		using var Savefile = FileAccess.Open(filename, FileAccess.ModeFlags.Write);
		Savefile.StoreString(AsBase64String());
	}

	public string AsBase64String() {
		QCircuit c = Compile();
		return Convert.ToBase64String(c.ToMatrix().ToByteArray());
	}

	public void AddPrecompiledGate(string filename) {
		var scene = GD.Load<PackedScene>("res://Scenes/Gates/PrecompiledGate.tscn");
		Node node = scene.Instantiate();
		if (!(node is GraphNode)) throw new ArgumentException("Can't add Scene as new Node because it isn't a GraphNode");
		node.Name = Guid.NewGuid().ToString();
		PrecompiledGate newNode = node as PrecompiledGate;
		Helpers.AddCloseButton(newNode, this);
		try {
			newNode.InitializeFromFile(filename);
			AddGraphNode(newNode);
		} catch (Exception ex) {
			EmitSignal(SignalName.CircuitCompileError, ex.Message);
			GD.PrintErr(ex);
		}

		
	}

}
