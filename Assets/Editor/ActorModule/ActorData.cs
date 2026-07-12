using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;

[CreateAssetMenu(fileName = "ActorData", menuName = "Custom/Actor")]
public class ActorData : EntityData
{
    public string desc;
    //public List<EntityModule> modules;
    public List<AbilityData> abilities;
    public LootTableData lootTable;

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(label)) label = name;
        if (abilities == null) abilities = new List<AbilityData>();
        //if (modules == null) modules = new List<EntityModule>();
    }
}
