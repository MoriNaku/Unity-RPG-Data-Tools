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

        var foldout = new Foldout
        {
            text = "Effects",
            value = false
        };

        foldout.Add(viewPort);

        return foldout;
    }
}
