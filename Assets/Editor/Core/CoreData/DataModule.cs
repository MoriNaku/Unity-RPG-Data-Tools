using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class DataModule
{
    public string tabName;
    public string displayName;
    public string dataPath;
    public MonoScript moduleType;
    public MonoScript viewType;
    public bool usesCustomView;
    public bool usesTagFiltering;
}
