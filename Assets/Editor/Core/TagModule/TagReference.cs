using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class TagReference
{
    private static Dictionary<string, CustomTag> _tagsById;
    public static void Init()
    {
        if (_tagsById != null) return;

        _tagsById = new Dictionary<string, CustomTag>();

        string[] guids = AssetDatabase.FindAssets("t:CustomTag");
        foreach(var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CustomTag tag = AssetDatabase.LoadAssetAtPath<CustomTag>(path);

            if (tag == null) continue;
            if (string.IsNullOrWhiteSpace(tag.id)) continue;

            _tagsById[tag.id] = tag;
        }
    }

    public static void Rebuild()
    {
        _tagsById = null;
        Init();
    }

    public static CustomTag Get(string id)
    {
        Init();

        if (string.IsNullOrWhiteSpace(id)) return null;

        if(!_tagsById.TryGetValue("tag_"+id.ToLower(), out var tag))
        {
            Debug.LogWarning($"Tag not found: tag_{id.ToLower()}");
            return null;
        }

        return tag;
    }
}
