using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

    
    #if UNITY_EDITOR
    [ContextMenu("Refresh Modules")]
    public void RefreshModules()
    {
        var dataTypes = TypeCache.GetTypesDerivedFrom<EntityData>()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsGenericType &&
                type.Name.EndsWith("Data"))
            .OrderBy(type => type.Name)
            .ToList();

        // Keep existing registry entries and their custom settings.
        var existing = moduleList
            .Where(entry => entry != null && entry.moduleType != null)
            .Select(entry => (
                entry,
                type: entry.moduleType.GetClass()))
            .Where(pair => pair.type != null)
            .GroupBy(pair => pair.type)
            .ToDictionary(group => group.Key, group => group.First().entry);

        // Resolve MonoScript references once.
        var scripts = new Dictionary<Type, MonoScript>();

        foreach (string guid in AssetDatabase.FindAssets("t:MonoScript"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            Type type = script?.GetClass();

            if (type != null && !scripts.ContainsKey(type))
                scripts.Add(type, script);
        }

        var refreshed = new List<DataModule>();

        foreach (Type dataType in dataTypes)
        {
            if (!scripts.TryGetValue(dataType, out MonoScript dataScript))
                continue;

            if (existing.TryGetValue(dataType, out DataModule entry))
            {
                refreshed.Add(entry);
                continue;
            }

            string name = dataType.Name[..^"Data".Length];

            Type viewType = dataType.Assembly.GetType(
                dataType.Namespace == null
                    ? name + "View"
                    : dataType.Namespace + "." + name + "View"
            );

            scripts.TryGetValue(viewType ?? typeof(void), out MonoScript viewScript);

            refreshed.Add(new DataModule
            {
                tabName = ObjectNames.NicifyVariableName(name),
                displayName = ObjectNames.NicifyVariableName(name),
                dataPath = $"Assets/Data/{name}s",
                moduleType = dataScript,
                viewType = viewScript,
                usesCustomView = viewScript != null,
                usesTagFiltering = true
            });
        }

        moduleList = refreshed;

        EditorUtility.SetDirty(this);
        AssetView.RefreshOpenWindows();
    }
    #endif
}
