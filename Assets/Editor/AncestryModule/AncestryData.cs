using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "AncestryData", menuName = "CryingForest/Ancestry")]
public class AncestryData : EntityData
{
    /*
     * 
     * You can add specific fields here
     * 
     * ex:
     * public string description;
     * public float value;
     * public bool condition;
     * 
     */
    public string description;
    public List<MechanicModule> mechanics = new List<MechanicModule>();

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(label)) label = name;
    }
}
