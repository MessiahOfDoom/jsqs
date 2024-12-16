using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

public partial class Main : GraphEdit
{

	//TODO implement a way to keep track of node order in the tree to a) prevent loops, and b) make sure the circuit stays sane and compilable 
	private System.Collections.Generic.Dictionary<StringName, List<StringName>> tree = new(); 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

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
		GD.Print(stopwatch.Elapsed);
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

	public void DisconnectNodeInternal(StringName fromNode, int fromPort, StringName toNode, int toPort) {
		DisconnectNode(fromNode, fromPort, toNode, toPort);
		if(tree.ContainsKey(fromNode)) {
			tree[fromNode].Remove(toNode);
		}
	}

	public void ConnectNodeInternal(StringName fromNode, int fromPort, StringName toNode, int toPort) {
		ConnectNode(fromNode, fromPort, toNode, toPort);
		if(!tree.ContainsKey(fromNode)) tree[fromNode] = new();
		tree[fromNode].Add(toNode);
	}

	public bool WouldContainLoop(string fromNode, int fromPort, string toNode, int toPort) {
		if(fromNode == toNode) return true;
		//TODO check for loops properly
		return false;
	}

}
