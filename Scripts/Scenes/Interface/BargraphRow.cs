using Godot;
using System;

public partial class BargraphRow : Control
{

	[Export]
	public Label bits;

	[Export]
	public ColorRect bar;

	[Export]
	public Label count;

	[Export]
	public Label percentage;

	public void initialize(int bit, int bitCount, int thisCount, int totalCount) {
		bits.Text = Convert.ToString(bit, 2).PadLeft(bitCount, '0');
		bar.Size = new(400.0f * thisCount / totalCount, 15);
		count.Text = Convert.ToString(thisCount);
		percentage.Text = string.Format("{0:0.00}", (100.0d * thisCount) / totalCount) + "%";
	}

}
