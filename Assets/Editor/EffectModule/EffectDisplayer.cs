using UnityEngine;
using UnityEngine.UIElements;

public class EffectDisplayer : DisplayerCore
{
    private readonly EffectModule effectModule;

    public EffectDisplayer(EffectModule module) : base(module)
    {
        this.effectModule = module;
    }
    public override VisualElement CraftView()
    {
        var viewPort = new VisualElement();

        return viewPort;
    }
}
