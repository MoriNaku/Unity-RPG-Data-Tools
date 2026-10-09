using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class MechanicData : EntityData
{
    
}

[Serializable]
public class VisionModule : MechanicData
{
    public enum VisionType
    {
        low_light,
        darkvision,
        greater_darkvision,
        tremor_sense,
        blood_sense
    }
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