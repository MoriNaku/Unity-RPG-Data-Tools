using UnityEngine;
using UnityEngine.UIElements;

public class ItemDisplayer : DisplayerCore
{
    private readonly ItemModule itemModule;

    public ItemDisplayer(ItemModule module) : base(module)
    {
        this.itemModule = module;
    }
    public override VisualElement CraftView()
    {
        var viewPort = new VisualElement();

        var foldout = new Foldout
        {
            text = "Items",
            value = false
        };

        foldout.Add(viewPort);

        return foldout;
    }
}
