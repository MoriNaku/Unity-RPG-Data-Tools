using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class MechanicData : EntityData
{
    
}

public enum VisionType
{
    low_light,
    darkvision,
    greater_darkvision,
    tremor_sense,
    blood_sense
}
public enum DiceType
{
    d4 = 4,
    d6 = 6,
    d8 = 8,
    d10 = 10,
    d12 = 12,
    d20 = 20,
    d100 = 100
}
public enum AttributeType
{
    Physical,
    Fire,
    Water,
    Wind,
    Earth,
    Light,
    Dark
}

[Serializable]
public class VisionModule : MechanicData
{
    public VisionType Type;
    public int Range;

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class MovementModule : MechanicData
{
    public int Speed;

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class VariableValueModule : MechanicData
{
    public DiceType dice;
    public int amount;
    public AttributeType attribute;

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class FlatValueModule : MechanicData
{
    public int amount;
    public AttributeType attribute;

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class ConditionModule : MechanicData
{
    bool IsNumber;

    bool GreaterThan;
    bool LessThan;
    bool EqualTo;
    bool Inverted;
    int value;
    MechanicData obj;

    public List<MechanicData> moduleList = new();

    public bool GetConditionResult(object a)
    {
        if(IsNumber)
        {
            if (EqualTo)
            {
                if(Inverted)
                {
                    if(value != (int)a)
                    {
                        return true;
                    }
                }
                else
                {
                    if (value == (int)a)
                    {
                        return true;
                    }
                }
            }
            if (GreaterThan)
            {
                if(EqualTo)
                {
                    if((int)a >= value)
                    {
                        return true;
                    }
                } else
                {
                    if((int)a > value)
                    {
                        return true;
                    }
                }
            }
            if (LessThan)
            {
                if(EqualTo)
                {
                    if ((int)a <= value) 
                    {
                        return true;
                    }
                } else
                {
                    if((int)a < value)
                    {
                        return true;
                    }
                }
            }
        } else
        {
            if(obj == (MechanicData)a)
            {
                return true;
            }
        }

        return false;
    }

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }
}