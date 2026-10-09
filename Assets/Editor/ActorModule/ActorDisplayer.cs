using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ActorDisplayer : DisplayerCore
{
    private readonly ActorModule actorModule;
    
    public ActorDisplayer(ActorModule module) : base(module)
    {
        this.actorModule = module;
    }
    public override VisualElement CraftView()
    {
        var viewPort = new VisualElement();

        var foldout = new Foldout
        {
            text = "Actors",
            value = false
        };

        foldout.Add(viewPort);

        return foldout;
    }
}
