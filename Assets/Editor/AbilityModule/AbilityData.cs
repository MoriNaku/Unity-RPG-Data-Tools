using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "AbilityData", menuName = "CryingForest/Ability")]
public class AbilityData : EntityData
{
    public string desc;
    [SerializeReference] public List<Effect> effects;

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(label)) label = name;
        if (effects == null) effects = new List<Effect>();
    }
}
