using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Effect
{
    public virtual List<CustomTag> tags => new();
    public virtual string DisplayName => GetType().Name;
    public virtual string Summary => "";
    public virtual string IconBasePath => "Assets/Sprites/";
    protected abstract string IconFileName { get; }
    public string IconPath => IconBasePath + IconFileName + ".png";
    public abstract void Execute(GameObject caster, GameObject target);
    public abstract Effect Clone();
}

[Serializable]
public class DamageEffect : Effect
{
    public int amount;

    public override void Execute(GameObject caster, GameObject target)
    {
        Debug.Log("Damage Effect used!");
    }
    public override Effect Clone()
    {
        return new DamageEffect()
        {
            amount = this.amount
        };
    }
    public override string DisplayName => "DMG";
    protected override string IconFileName => "dmg";
    public override string Summary => $"DMG Amount: {amount}";
}

[Serializable]
public class DefenseEffect : Effect
{
    public int amount;

    public override void Execute(GameObject caster, GameObject target)
    {
        Debug.Log("Defense Effect used!");
    }
    public override Effect Clone()
    {
        return new DefenseEffect()
        {
            amount = this.amount
        };
    }
    public override string DisplayName => "DEF";
    protected override string IconFileName => "def";
    public override string Summary => $"DEF Value: {amount}";
}

[Serializable]
public class KnockbackEffect : Effect
{
    public float force;
    public override void Execute(GameObject caster, GameObject target)
    {
        Debug.Log("Knockback Effect used!");
    }
    public override Effect Clone()
    {
        return new KnockbackEffect()
        {
            force = this.force
        };
    }
    public override string DisplayName => "Knockback";
    protected override string IconFileName => "knockback";
    public override string Summary => $"Knockback Force: {force}";
}

[Serializable]
public class BurnEffect : Effect
{
    public float chance;
    public override void Execute(GameObject caster, GameObject target)
    {
        Debug.Log("Burn Effect used!");
    }
    public override Effect Clone()
    {
        return new BurnEffect()
        {
            chance = this.chance
        };
    }

    public override List<CustomTag> tags => new List<CustomTag> { TagReference.Get("Fire") };
    public override string DisplayName => "Burn";
    protected override string IconFileName => "burn";
    public override string Summary => $"Burn Chance: {chance:P0}";
}

[Serializable]
public class ParalyzeEffect : Effect
{
    public float chance;
    public override void Execute(GameObject caster, GameObject target)
    {
        Debug.Log("Paralyze Effect used!");
    }
    public override Effect Clone()
    {
        return new ParalyzeEffect()
        {
            chance = this.chance
        };
    }
    public override string DisplayName => "Paralyze";
    protected override string IconFileName => "paralyze";
    public override string Summary => $"Paralysis Chance: {chance:P0}";
}

[Serializable]
public class PoisonEffect : Effect
{
    public float chance;
    public override void Execute(GameObject caster, GameObject target)
    {
        Debug.Log("Poison Effect used!");
    }
    public override Effect Clone()
    {
        return new PoisonEffect()
        {
            chance = this.chance
        };
    }
    public override string DisplayName => "Poison";
    protected override string IconFileName => "poison";
    public override string Summary => $"Poison Chance: {chance:P0}";
}

[Serializable]
public class HealEffect : Effect
{
    public int amount;
    public override void Execute(GameObject caster, GameObject target)
    {
        Debug.Log("Heal Effect used!");
    }
    public override Effect Clone()
    {
        return new HealEffect()
        {
            amount = this.amount
        };
    }
    public override string DisplayName => "Heal";
    protected override string IconFileName => "heal";
    public override string Summary => $"Heal Amount: {amount}";
}