using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AbilityDisplayer : DisplayerCore
{
    private readonly AbilityModule abilityModule;

    public AbilityDisplayer(AbilityModule abilityModule) : base(abilityModule)
    {
        this.abilityModule = abilityModule;
    }

    private List<AbilityData> allAbilities = new();
    private ScrollView currentAbilityList = new ScrollView();

    public override VisualElement CraftView()
    {
        GatherAbilities();

        var viewPort = new VisualElement();
        var currentLabel = new Label();
        currentLabel.text = "Known Abilities";
        currentLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        currentLabel.style.fontSize = 18;
        viewPort.Add(currentLabel);

        currentAbilityList = new ScrollView();
        currentAbilityList.contentContainer.style.flexDirection = FlexDirection.Row;
        currentAbilityList.contentContainer.style.flexWrap = Wrap.Wrap;
        currentAbilityList.contentContainer.style.paddingLeft = 4;
        currentAbilityList.contentContainer.style.paddingRight = 4;
        currentAbilityList.contentContainer.style.paddingTop = 4;
        viewPort.Add(currentAbilityList);

        for (int i = 0; i < abilityModule.abilities.Count; i++)
        {
            currentAbilityList.Add(CreateAbilityList(abilityModule.abilities[i], i));
        }

        if (abilityModule.abilities.Count < 1)
        {
            currentAbilityList.Add(new Label("None"));
        }

        //All Abilities View
        var allAbilitiesLabel = new Label();
        allAbilitiesLabel.text = "All Abilities";
        allAbilitiesLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        allAbilitiesLabel.style.fontSize = 18;
        viewPort.Add(allAbilitiesLabel);

        var abilityList = new ScrollView();
        abilityList.contentContainer.style.flexDirection = FlexDirection.Row;
        abilityList.contentContainer.style.flexWrap = Wrap.Wrap;
        abilityList.contentContainer.style.paddingLeft = 4;
        abilityList.contentContainer.style.paddingRight = 4;
        abilityList.contentContainer.style.paddingTop = 4;
        viewPort.Add(abilityList);

        PopulateAbilities(abilityList);

        return viewPort;
    }

    private void GatherAbilities()
    {
        string[] guids = AssetDatabase.FindAssets("t:AbilityData", new[] { "Assets/Data/Abilities" });

        allAbilities = new List<AbilityData>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AbilityData ability = AssetDatabase.LoadAssetAtPath<AbilityData>(path);

            if (ability != null)
            {
                allAbilities.Add(ability);
            }
        }
    }

    private VisualElement CreateAbilityList(AbilityData data, int index)
    {
        var button = new Button(() => AbilityView.Open(data));
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

        var del = new Button(() => RemoveAbilityAt(index));
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
            var button = new Button(() => AddAbility(a));
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
    private void RemoveAbilityAt(int index)
    {
        abilityModule.abilities.RemoveAt(index);

        RefreshCurrentAbilities();
    }
    private void RefreshCurrentAbilities()
    {
        currentAbilityList.Clear();

        for (int i = 0; i < abilityModule.abilities.Count; i++)
        {
            currentAbilityList.Add(CreateAbilityList(abilityModule.abilities[i], i));
        }
    }
    private void AddAbility(AbilityData data)
    {
        abilityModule.abilities.Add(data);

        RefreshCurrentAbilities();
    }
}
