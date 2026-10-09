using System;
using System.Collections.Generic;
using System.Reflection;
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

    private static readonly Dictionary<Type, Type> displayerTypes = new();

    static DisplayerCore()
    {
        foreach (Type type in TypeCache.GetTypesDerivedFrom<DisplayerCore>())
        {
            if (type.IsAbstract || type.ContainsGenericParameters)
                continue;

            foreach (ConstructorInfo constructor in type.GetConstructors())
            {
                ParameterInfo[] parameters = constructor.GetParameters();

                if (parameters.Length != 1)
                    continue;

                Type moduleType = parameters[0].ParameterType;

                if (!typeof(EntityModule).IsAssignableFrom(moduleType))
                    continue;

                if (displayerTypes.ContainsKey(moduleType))
                {
                    Debug.LogWarning(
                        $"Multiple displayers registered for {moduleType.Name}.");
                    continue;
                }

                displayerTypes.Add(moduleType, type);
            }
        }
    }

    public static DisplayerCore GetDisplayer(EntityModule mod)
    {
        if (mod == null)
            return null;

        Type moduleType = mod.GetType();

        if (!displayerTypes.TryGetValue(moduleType, out Type displayerType))
        {
            Debug.LogError(
                $"No DisplayerCore implementation found for {moduleType.Name}.");
            return null;
        }

        return (DisplayerCore)Activator.CreateInstance(displayerType, mod);

        /*
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
            case "AncestryModule":
                return new AncestryDisplayer(mod as AncestryModule);
            case "MechanicModule":
                return new MechanicDisplayer(mod as MechanicModule);
            default:
                return new TagDisplayer(mod as TagModule);
        }
        */
    }
}