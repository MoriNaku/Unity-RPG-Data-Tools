using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MechanicDisplayer : DisplayerCore
{
    private readonly MechanicModule mechanicModule;

    public MechanicDisplayer(MechanicModule mechanicModule) : base(mechanicModule)
    {
        this.mechanicModule = mechanicModule;
    }

    private List<Type> allMechanics = new();
    private ScrollView currentMechanicList = new ScrollView();

    public override VisualElement CraftView()
    {
        GatherMechanics();

        var viewPort = new VisualElement();
        var currentLabel = new Label();
        currentLabel.text = "Active Mechanics";
        currentLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        currentLabel.style.fontSize = 18;
        viewPort.Add(currentLabel);

        currentMechanicList = new ScrollView();
        currentMechanicList.contentContainer.style.flexDirection = FlexDirection.Column;
        currentMechanicList.contentContainer.style.flexWrap = Wrap.NoWrap;
        currentMechanicList.contentContainer.style.paddingLeft = 4;
        currentMechanicList.contentContainer.style.paddingRight = 4;
        currentMechanicList.contentContainer.style.paddingTop = 4;
        viewPort.Add(currentMechanicList);

        for (int i = 0; i < mechanicModule.mechanics.Count; i++)
        {
            currentMechanicList.Add(CreateMechanicList(mechanicModule.mechanics[i], i));
        }

        if (mechanicModule.mechanics.Count < 1)
        {
            currentMechanicList.Add(new Label("None"));
        }

        //All Abilities View
        var allAbilitiesLabel = new Label();
        allAbilitiesLabel.text = "All Mechanics";
        allAbilitiesLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        allAbilitiesLabel.style.fontSize = 18;
        viewPort.Add(allAbilitiesLabel);

        var mechanicList = new ScrollView();
        mechanicList.contentContainer.style.flexDirection = FlexDirection.Row;
        mechanicList.contentContainer.style.flexWrap = Wrap.Wrap;
        mechanicList.contentContainer.style.paddingLeft = 4;
        mechanicList.contentContainer.style.paddingRight = 4;
        mechanicList.contentContainer.style.paddingTop = 4;
        viewPort.Add(mechanicList);

        PopulateMechanics(mechanicList);

        var foldout = new Foldout
        {
            text = "Mechanics",
            value = false
        };

        foldout.Add(viewPort);

        return foldout;
    }

    private void GatherMechanics()
    {
        allMechanics = TypeCache.GetTypesDerivedFrom<MechanicData>()
        .Where(type =>
            !type.IsAbstract && 
            !type.IsGenericType &&
            type.GetConstructor(Type.EmptyTypes) != null
        )
        .OrderBy(type => type.Name)
        .ToList();
    }

    private VisualElement CreateMechanicList(MechanicData data, int index)
    {
        var button = new VisualElement();
        button.style.alignItems = Align.Center;
        button.style.justifyContent = Justify.Center;
        button.style.flexDirection = FlexDirection.Row;
        button.style.flexGrow = 1;

        button.style.marginTop = 6;
        button.style.marginBottom = 6;
        button.style.marginLeft = 6;
        button.style.marginRight = 6;

        button.style.paddingTop = 6;
        button.style.paddingBottom = 6;
        button.style.paddingLeft = 6;
        button.style.paddingRight = 6;

        button.style.borderTopWidth = 1;
        button.style.borderBottomWidth = 1;
        button.style.borderLeftWidth = 1;
        button.style.borderRightWidth = 1;

        var borderColor = new Color(0.35f, 0.35f, 0.35f);

        button.style.borderTopColor = borderColor;
        button.style.borderBottomColor = borderColor;
        button.style.borderLeftColor = borderColor;
        button.style.borderRightColor = borderColor;

        var defZone = new VisualElement();
        defZone.style.alignItems = Align.Center;
        defZone.style.justifyContent = Justify.Center;
        defZone.style.flexDirection = FlexDirection.Column;
        defZone.style.width = 120;
        defZone.style.flexShrink = 0;
        defZone.style.borderRightWidth = 1;
        defZone.style.borderRightColor = borderColor;

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
        icon.style.alignItems = Align.Center;
        icon.style.justifyContent = Justify.Center;

        var name_index = data.GetType().Name.IndexOf("Module");
        var label = new Label(data.GetType().Name.Remove(name_index));
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.fontSize = 10;

        var del = new Button(() => RemoveMechanicAt(index));
        del.style.width = 80;
        del.style.height = 16;
        del.text = "Delete";

        defZone.Add(icon);
        defZone.Add(label);
        defZone.Add(del);

        var contZone = new VisualElement();
        contZone.style.flexDirection = FlexDirection.Column;
        contZone.style.flexGrow = 1;
        contZone.style.marginLeft = 12;
        contZone.style.marginRight = 8;

        // Derive contents
        contZone.Add(CreateMechanicFields(data));

        button.Add(defZone);
        button.Add(contZone);

        return button;
    }
    private VisualElement CreateMechanicFields(MechanicData mechanic)
    {
        var container = new VisualElement();

        var fields = mechanic.GetType().GetFields(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly
        );

        foreach (var field in fields)
        {
            VisualElement element = null;

            if (field.FieldType == typeof(int))
            {
                var input = new IntegerField(field.Name)
                {
                    value = (int)field.GetValue(mechanic)
                };

                input.RegisterValueChangedCallback(evt =>
                {
                    field.SetValue(mechanic, evt.newValue);
                });

                element = input;
            }
            else if (field.FieldType == typeof(float))
            {
                var input = new FloatField(field.Name)
                {
                    value = (float)field.GetValue(mechanic)
                };

                input.RegisterValueChangedCallback(evt =>
                {
                    field.SetValue(mechanic, evt.newValue);
                });

                element = input;
            }
            else if (field.FieldType == typeof(bool))
            {
                var input = new Toggle(field.Name)
                {
                    value = (bool)field.GetValue(mechanic)
                };

                input.RegisterValueChangedCallback(evt =>
                {
                    field.SetValue(mechanic, evt.newValue);
                });

                element = input;
            }
            else if (field.FieldType.IsEnum)
            {
                var input = new EnumField(
                    field.Name,
                    (Enum)field.GetValue(mechanic)
                );

                input.RegisterValueChangedCallback(evt =>
                {
                    field.SetValue(mechanic, evt.newValue);
                });

                element = input;
            }

            if (element != null)
            {
                container.Add(element);
            }
        }

        return container;
    }
    private void PopulateMechanics(VisualElement list)
    {
        foreach (var a in allMechanics)
        {
            var button = new Button(() => AddMechanic(a));
            button.style.width = 88;
            button.style.height = 88;
            button.style.marginRight = 6;
            button.style.marginBottom = 6;
            button.style.alignItems = Align.Center;
            button.style.justifyContent = Justify.Center;
            button.style.flexDirection = FlexDirection.Column;

            var name_index = a.Name.IndexOf("Module");
            var label = new Label(a.Name.Remove(name_index));
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.fontSize = 10;

            button.Add(label);

            list.Add(button);
        }
    }
    private void RemoveMechanicAt(int index)
    {
        mechanicModule.mechanics.RemoveAt(index);

        RefreshCurrentMechanics();
    }
    private void RefreshCurrentMechanics()
    {
        currentMechanicList.Clear();

        for (int i = 0; i < mechanicModule.mechanics.Count; i++)
        {
            currentMechanicList.Add(CreateMechanicList(mechanicModule.mechanics[i], i));
        }
    }
    private void AddMechanic(Type data)
    {
        MechanicData mechanic = (MechanicData)ScriptableObject.CreateInstance(data);

        mechanicModule.mechanics.Add(mechanic);

        RefreshCurrentMechanics();
    }
}
