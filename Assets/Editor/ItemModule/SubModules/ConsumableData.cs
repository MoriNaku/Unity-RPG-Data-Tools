using System;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ConsumableData", menuName = "Custom/Items/Consumable")]
public class ConsumableData : ItemData
{
    public int stack;
    public int maxStack;
    public bool isReuseable;
    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }
}