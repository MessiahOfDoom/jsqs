using Godot;
using System;

[Tool, GlobalClass]
public partial class SubScriptBBCode : RichTextEffect
{

    string bbcode = "sub";

    public override bool _ProcessCustomFX(CharFXTransform charFX)
    {
        charFX.Transform = charFX.Transform.ScaledLocal(new(0.625f, 0.625f)).TranslatedLocal(new(0, 8));
        return base._ProcessCustomFX(charFX);
    }


}
