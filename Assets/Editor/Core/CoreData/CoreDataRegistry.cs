using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tools/Core Data Registry")]
public class CoreDataRegistry : ScriptableObject
{
    public List<DataModule> moduleList = new();

    private void OnValidate()
    {
        #if UNITY_EDITOR
            AssetView.RefreshOpenWindows();
        #endif
    }
}
