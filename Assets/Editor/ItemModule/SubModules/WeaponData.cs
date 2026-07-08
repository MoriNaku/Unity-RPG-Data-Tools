using System;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Custom/Items/Weapon")]
public class WeaponData : EquipData
{
    [Filterable] public WeaponType weaponType;

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }
    public override void EnsureDefaults()
    {
        if (!effects.Exists(e => e is DamageEffect))
            effects.Add(new DamageEffect());
    }
}