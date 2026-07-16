using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*
 * Copy this file and rename it.
 * Replace every occurrence of "Example" with your own type.
 * Remove this comment once your implementation is complete.
 */

[CreateAssetMenu(fileName = "ExampleData", menuName = "CryingForest/Example")]
public class ExampleData : EntityData
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

    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        throw new NotImplementedException();
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(label)) label = name;
    }
}
