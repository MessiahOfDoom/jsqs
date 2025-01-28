using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MonteCarloGraph : Control
{

	[Export]
	public PackedScene BargraphRowScene;
	
	[Export]
	public VBoxContainer container;


	private bool showAllStates = false;
	private Dictionary<int, int> lastStates = null;
	private int lastRuns = -1;
	private int lastBitCount = -1;

	public override void _Ready()
    {
		
    }

    public void displayGraph(Dictionary<int, int> values, int totalRuns, int bitCount) {
		lastStates = values;
		lastRuns = totalRuns;
		lastBitCount = bitCount;
		foreach(var c in container.GetChildren()){
			container.RemoveChild(c);
		}
		if(showAllStates) {
			for(int i = 0; i < 1 << bitCount; ++i) {
				AppendBar(i, bitCount, values.GetValueOrDefault(i, 0), totalRuns);
			}
		}else {
			foreach(var pair in values.OrderBy(x => x.Key)) {
				AppendBar(pair.Key, bitCount, pair.Value, totalRuns);
			}
		}
	}

	private void AppendBar(int bit, int bitCount, int thisCount, int totalCount) {
		var row = BargraphRowScene.Instantiate() as BargraphRow;
		row.initialize(bit, bitCount, thisCount, totalCount);
		container.AddChild(row);
	}

	public void ToggleShowAllStates(bool showAllStates) {
		this.showAllStates = showAllStates;
		if(lastStates != null) {
			displayGraph(lastStates, lastRuns, lastBitCount);
		}
	}
}
