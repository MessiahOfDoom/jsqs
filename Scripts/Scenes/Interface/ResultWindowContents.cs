using Godot;
using System;

public partial class ResultWindowContents : TabContainer
{
	
	private QCircuit lastCompiledCircuit = null;
	private int lastQBitCount = -1;

	public void setCircuit(QCircuit circuit, int qbits) {
		lastCompiledCircuit = circuit;
		lastQBitCount = qbits;
		rerunMonteCarlo();
	}

	public void rerunMonteCarlo() {
		rerunMonteCarlo(1000);
	}
	public void rerunMonteCarlo(int runs) {
		var mc = lastCompiledCircuit.MonteCarlo(runs, 0);
		var graph = GetTree().Root.FindChild("MonteCarloGraph", recursive:true, owned:false) as MonteCarloGraph;
		graph.displayGraph(mc, runs, lastQBitCount);
	}

}
