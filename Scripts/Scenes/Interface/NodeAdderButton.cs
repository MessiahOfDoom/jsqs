using Godot;
using System;
using System.ComponentModel;

[Tool, GlobalClass]
public partial class NodeAdderButton : Button {

    [Export]
    public PackedScene Gate;

    [Export]
    public SideBar parent;

    public override void _Ready()
    {
        Connect("pressed", Callable.From(() => {
            parent.EmitSignal(SideBar.SignalName.AddNode, Gate);
        }));
    }

}