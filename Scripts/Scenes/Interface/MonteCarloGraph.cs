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
	public override void _Ready()
    {
		
    }

    public void displayGraph(Dictionary<int, int> values, int totalRuns, bool displayZeros, int bitCount) {
		foreach(var c in container.GetChildren()){
			container.RemoveChild(c);
		}
		if(displayZeros) {
			for(int i = 0; i < 2 << bitCount; ++i) {
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

	
}
