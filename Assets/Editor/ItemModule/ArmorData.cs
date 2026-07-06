using System;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ArmorData", menuName = "Custom/Items/Armor")]
public class ArmorData : EquipData
{
    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }
    public override void EnsureDefaults()
    {
        if (!effects.Exists(e => e is DefenseEffect))
            effects.Add(new DefenseEffect());
    }
}