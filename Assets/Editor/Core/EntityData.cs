using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;

public abstract class EntityData : ScriptableObject
{
    public string id;
    public string label;
    public List<CustomTag> manualTags = new();
    public List<CustomTag> tagList = new();

    public abstract VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false);
}
