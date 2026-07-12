using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class DisplayerCore
{
    protected readonly EntityModule module;

    public EntityModule Module => module;

    protected DisplayerCore(EntityModule module)
    {
        this.module = module;
    }
    public abstract VisualElement CraftView();

    public static DisplayerCore GetDisplayer(EntityModule mod)
    {
        switch (mod.GetType().FullName)
        {
            case "AbilityModule":
                return new AbilityDisplayer(mod as AbilityModule);
            case "ActorModule":
                return new ActorDisplayer(mod as ActorModule);
            case "ItemModule":
                return new ItemDisplayer(mod as ItemModule);
            case "EffectModule":
                return new EffectDisplayer(mod as EffectModule);
            default:
                return new TagDisplayer(mod as TagModule);
        }
    }
}