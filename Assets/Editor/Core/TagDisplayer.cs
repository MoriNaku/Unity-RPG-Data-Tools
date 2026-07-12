using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TagDisplayer : DisplayerCore
{
    private readonly TagModule tagModule;
    private ScrollView tagLeft;
    private List<CustomTag> allTags = new();

    public TagDisplayer(TagModule module) : base(module)
    {
        this.tagModule = module;
    }

    public override VisualElement CraftView()
    {
        GatherTags();

        //Tags
        var splitViewT = new TwoPaneSplitView(0, 315, TwoPaneSplitViewOrientation.Horizontal);
        splitViewT.style.flexGrow = 1;

        tagLeft = new ScrollView();
        for (var i = 0; i < tagModule.auto.Count; i++)
        {
            var tag = tagModule.auto[i];
            tagLeft.Add(tag.GetVisuals(() => TagView.Open(tag), null, false));
        }
        for (int i = 0; i < tagModule.manual.Count; i++)
        {
            var tag = tagModule.manual[i];
            int index = i;
            tagLeft.Add(tag.GetVisuals(() => TagView.Open(tag), () => RemoveTagAt(index), true));
        }
        splitViewT.Add(tagLeft);

        var tagRight = new VisualElement();
        tagRight.style.flexDirection = FlexDirection.Row;
        tagRight.style.flexWrap = Wrap.Wrap;
        tagRight.style.paddingLeft = 4;
        tagRight.style.paddingBottom = 4;
        tagRight.style.paddingTop = 4;
        splitViewT.Add(tagRight);

        foreach (var tag in allTags)
        {
            tagRight.Add(tag.GetVisuals(() => AddTag(tag), null, false));
        }
        return splitViewT;
    }
    private void GatherTags()
    {
        string[] guidsT = AssetDatabase.FindAssets("t:CustomTag", new[] { "Assets/Data/Tags" });
        allTags = new List<CustomTag>();
        foreach (string guid in guidsT)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CustomTag tag = AssetDatabase.LoadAssetAtPath<CustomTag>(path);

            if (tag != null)
            {
                allTags.Add(tag);
            }
        }
    }

    private void AddTag(CustomTag data)
    {
        tagModule.manual.Add(data);

        RefreshCurrentTags();
    }
    private void RemoveTagAt(int index)
    {
        tagModule.manual.RemoveAt(index);

        RefreshCurrentTags();
    }
    private void RefreshCurrentTags()
    {
        tagLeft.Clear();

        for (var i = 0; i < tagModule.auto.Count; i++)
        {
            var tag = tagModule.auto[i];
            tagLeft.Add(tag.GetVisuals(() => TagView.Open(tag), null, false));
        }
        for (int i = 0; i < tagModule.manual.Count; i++)
        {
            var tag = tagModule.manual[i];
            var index = i;
            tagLeft.Add(tag.GetVisuals(() => TagView.Open(tag), () => RemoveTagAt(index), true));
        }
    }
}
