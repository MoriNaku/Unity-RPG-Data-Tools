using UnityEngine;
using System;
using System.Collections.Generic;

public enum WeaponType
{
    Sword,
    Spear,
    Axe
}
public enum EquipType
{
    Helm,
    Chest,
    Legging,
    Boots,
    Gloves,
    Accessory,
    Weapon
}
public enum ItemType
{
    Key,
    Equip,
    Consumable
}
public abstract class ItemData : EntityData
{
    public string desc;
    public Sprite icon;
    [Filterable] public ItemType itemType;
    
    [SerializeReference] public List<Effect> effects;

    public virtual void EnsureDefaults() { }
}

public abstract class EquipData : ItemData
{
    [Filterable] public EquipType equipType;
}





