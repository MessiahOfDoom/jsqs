using Godot;
using System;

public partial class ExpandableVBoxContainer : VBoxContainer
{
	[Export]
	public VBoxContainer container;
	[Export]
	public TextureButton btn;
	[Export]
	public Texture2D expand;
	[Export]
	public Texture2D expanded;
	[Export]
	public Label titleLabel;
	[Export]
	public string title;

    public override void _Ready()
    {
        titleLabel.Text = title;
    }
    public void OnButtonPressed() {
		if(container != null) {
			container.Visible = !container.Visible;
			if(btn != null) {
				btn.TextureNormal = container.Visible ? expanded : expand;
			}
		}
	}
}
