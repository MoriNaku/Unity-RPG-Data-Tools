using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;

public abstract class EntityData : ScriptableObject
{
    public string id;
    public string label;
    public Sprite icon;
    public TagModule tags = new();

    [SerializeReference]
    public List<EntityModule> modules = new();

    public abstract VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false);
}
