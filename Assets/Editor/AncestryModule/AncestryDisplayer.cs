using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AncestryDisplayer : DisplayerCore
{
    private readonly AncestryModule ancestryModule;

    public AncestryDisplayer(AncestryModule ancestryModule) : base(ancestryModule)
    {
        this.ancestryModule = ancestryModule;
    }

    private List<AncestryData> allAbilities = new();
    private ScrollView currentAncestryList = new ScrollView();

    public override VisualElement CraftView()
    {
        GatherAbilities();

        var viewPort = new VisualElement();
        var currentLabel = new Label();
        currentLabel.text = "Assigned Ancestries";
        currentLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        currentLabel.style.fontSize = 18;
        viewPort.Add(currentLabel);

        currentAncestryList = new ScrollView();
        currentAncestryList.contentContainer.style.flexDirection = FlexDirection.Row;
        currentAncestryList.contentContainer.style.flexWrap = Wrap.Wrap;
        currentAncestryList.contentContainer.style.paddingLeft = 4;
        currentAncestryList.contentContainer.style.paddingRight = 4;
        currentAncestryList.contentContainer.style.paddingTop = 4;
        viewPort.Add(currentAncestryList);

        for (int i = 0; i < ancestryModule.ancestries.Count; i++)
        {
            currentAncestryList.Add(CreateAncestryList(ancestryModule.ancestries[i], i));
        }

        if (ancestryModule.ancestries.Count < 1)
        {
            currentAncestryList.Add(new Label("None"));
        }

        //All Abilities View
        var allAbilitiesLabel = new Label();
        allAbilitiesLabel.text = "All Ancestries";
        allAbilitiesLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        allAbilitiesLabel.style.fontSize = 18;
        viewPort.Add(allAbilitiesLabel);

        var ancestryList = new ScrollView();
        ancestryList.contentContainer.style.flexDirection = FlexDirection.Row;
        ancestryList.contentContainer.style.flexWrap = Wrap.Wrap;
        ancestryList.contentContainer.style.paddingLeft = 4;
        ancestryList.contentContainer.style.paddingRight = 4;
        ancestryList.contentContainer.style.paddingTop = 4;
        viewPort.Add(ancestryList);

        PopulateAbilities(ancestryList);

        var foldout = new Foldout
        {
            text = "Ancestries",
            value = false
        };

        foldout.Add(viewPort);

        return foldout;
    }

    private void GatherAbilities()
    {
        string[] guids = AssetDatabase.FindAssets("t:AncestryData", new[] { "Assets/Data/Ancestries" });

        allAbilities = new List<AncestryData>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AncestryData ancestry = AssetDatabase.LoadAssetAtPath<AncestryData>(path);

            if (ancestry != null)
            {
                allAbilities.Add(ancestry);
            }
        }
    }

    private VisualElement CreateAncestryList(AncestryData data, int index)
    {
        var button = new Button(() => AncestryView.Open(data));
        button.style.width = 88;
        button.style.height = 88;
        button.style.marginRight = 6;
        button.style.marginBottom = 6;
        button.style.alignItems = Align.Center;
        button.style.justifyContent = Justify.Center;
        button.style.flexDirection = FlexDirection.Column;

        var icon = new Image();
        if (data.icon == null)
        {
            icon.image = null;
        }
        else
        {
            icon.image = data.icon?.texture;
        }
        icon.scaleMode = ScaleMode.ScaleToFit;
        icon.style.width = 40;
        icon.style.height = 40;
        icon.style.marginBottom = 4;

        var label = new Label(data.label);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.fontSize = 10;

        var del = new Button(() => RemoveAncestryAt(index));
        del.style.width = 80;
        del.style.height = 16;
        del.text = "Delete";

        button.Add(icon);
        button.Add(label);
        button.Add(del);

        return button;
    }
    private void PopulateAbilities(VisualElement list)
    {
        foreach (var a in allAbilities)
        {
            var button = new Button(() => AddAncestry(a));
            button.style.width = 88;
            button.style.height = 88;
            button.style.marginRight = 6;
            button.style.marginBottom = 6;
            button.style.alignItems = Align.Center;
            button.style.justifyContent = Justify.Center;
            button.style.flexDirection = FlexDirection.Column;

            var icon = new Image();
            if (a.icon != null)
                icon.image = a.icon.texture;
            icon.scaleMode = ScaleMode.ScaleToFit;
            icon.style.width = 40;
            icon.style.height = 40;
            icon.style.marginBottom = 4;

            var label = new Label(a.name);
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.fontSize = 10;

            button.Add(icon);
            button.Add(label);

            list.Add(button);
        }
    }
    private void RemoveAncestryAt(int index)
    {
        ancestryModule.ancestries.RemoveAt(index);

        RefreshCurrentAncestries();
    }
    private void RefreshCurrentAncestries()
    {
        currentAncestryList.Clear();

        for (int i = 0; i < ancestryModule.ancestries.Count; i++)
        {
            currentAncestryList.Add(CreateAncestryList(ancestryModule.ancestries[i], i));
        }
    }
    private void AddAncestry(AncestryData data)
    {
        ancestryModule.ancestries.Add(data);

        RefreshCurrentAncestries();
    }
}
