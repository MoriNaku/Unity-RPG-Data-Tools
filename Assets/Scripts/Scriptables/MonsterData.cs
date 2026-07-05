using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Custom/Monster")]
public class MonsterData : EntityData
{
    public string desc;
    public Sprite icon;
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
    }
}
