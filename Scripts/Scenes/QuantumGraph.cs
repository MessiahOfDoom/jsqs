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
	public override void _Ready()
	{

		if(inputGate != null) {
			nodes.Add(inputGate);
			tree.Add(inputGate.Name, new());
		}	
/*
		var testCount = 10	;
		var testLen = 1 << testCount;

		var v = new Vector(testLen);
		v[0] = 1;
		var stopwatch = Stopwatch.StartNew();
		var m = GateBuilder.Identity(testCount) * GateBuilder.Hadamard(testCount) * GateBuilder.Identity(testCount);
		stopwatch.Stop();
		GD.Print(stopwatch.Elapsed);
		GD.Print(m.getN());
		stopwatch.Restart();
		var v2 = m * v;
		stopwatch.Stop();
		GD.Print(stopwatch.Elapsed);
		m.EvalCache();
		stopwatch.Restart();
		var v3 = GateBuilder.Hadamard(testCount) * (GateBuilder.Identity(testCount) * (GateBuilder.Hadamard(testCount) * (GateBuilder.Identity(testCount) * v)));
		stopwatch.Stop();
		GD.Print(v3[0]);
		GD.Print(stopwatch.Elapsed);*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnConnectionRequest(string fromNode, int fromPort, string toNode, int toPort) {
		var conn = GetConnection(fromNode, fromPort, true);
		if(conn != null) DisconnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"]);
		conn = GetConnection(toNode, toPort, false);
		if(conn != null) DisconnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"]);
		RebuildTree();
		if(!WouldContainLoop(fromNode, fromPort, toNode, toPort)) ConnectNodeInternal(fromNode, fromPort, toNode, toPort);
	}

	public void OnDisconnectionRequest(string fromNode, int fromPort, string toNode, int toPort) {
		DisconnectNodeInternal(fromNode, fromPort, toNode, toPort);
		RebuildTree();
	}

	public void OnConnectionToFromEmpty(string node, int port, Vector2 pos, bool from) {
		var conn = GetConnection(node, port, from);
		if(conn != null) DisconnectNodeInternal((StringName)conn["from_node"], (int)conn["from_port"], (StringName)conn["to_node"], (int)conn["to_port"]);
		RebuildTree();
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

	public void DisconnectNodeInternal(StringName fromNode, int fromPort, StringName toNode, int toPort) {
		DisconnectNode(fromNode, fromPort, toNode, toPort);
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
	}

	public void RemoveNode(GraphNode node) {
		nodes.Remove(node);
		tree.Remove(node.Name);
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
		Helpers.AddCloseButton(newNode, this);
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

	private void PrintTree() {
		GD.Print("\n\n\n");
		foreach(GraphNode node in nodes) {
			GD.Print(node.Name);
			foreach(var name in tree[node.Name]) {
				GD.Print("\t" + name);
			}
			GD.Print("__________________________");
		}
	}

}
