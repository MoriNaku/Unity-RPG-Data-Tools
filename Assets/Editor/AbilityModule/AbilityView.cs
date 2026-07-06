using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using TMPro.EditorUtilities;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class AbilityView : EditorWindow
{
    private List<CustomTag> allTags;
    private List<Type> allEffectTypes;
    private VisualElement abilityRight;
    private ScrollView effectList;
    private ScrollView tagLeft;
    private Image preview;

    private TextField _label;
    private TextField _desc;
    private UnityEditor.UIElements.ObjectField _icon;
    private List<Effect> currentEffects = new();
    private Dictionary<string, List<CustomTag>> currentTags = new();

    private AbilityData _obj;
    private SerializedObject _serializedObj;
    private SerializedProperty _labelProp;
    private SerializedProperty _descProp;
    private SerializedProperty _iconProp;
    private SerializedProperty _effectsProp;
    private SerializedProperty _tagProp;
    private SerializedProperty _manualProp;

    private EffectHost draftHost;
    private SerializedObject draftSerializedObject;
    private SerializedProperty draftEffectProperty;

    private void OnSelectionChange()
    {
        if(Selection.activeObject is AbilityData ability)
        {
            SetAbility(ability);
        }
    }
    private void OnFocus()
    {
        rootVisualElement.Clear();
        if (_obj == null || _serializedObj == null)
        {
            currentTags = new();
            currentEffects = new();
        }
        CreateGUI();
    }
    public static void Open(AbilityData ability)
    {
        var window = GetWindow<AbilityView>("AbilityView");
        window.Focus();
        window.SetAbility(ability);
    }
    private void SetAbility(AbilityData ability)
    {
        _obj = ability;

        if (_obj != null)
        {
            currentTags = new();
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
            foreach(var auto in _obj.tagList)
            {
                currentTags["auto"].Add(auto);
            }
            foreach(var manual in _obj.manualTags)
            {
                currentTags["manual"].Add(manual);
            }
            currentEffects = _obj.effects ?? new List<Effect>();
            _serializedObj = new SerializedObject(_obj);
            _labelProp = _serializedObj.FindProperty("label");
            _descProp = _serializedObj.FindProperty("desc");
            _iconProp = _serializedObj.FindProperty("icon");
            _effectsProp = _serializedObj.FindProperty("effects");
            _tagProp = _serializedObj.FindProperty("tagList");
            _manualProp = _serializedObj.FindProperty("manualTags");
        }
        else
        {
            currentTags = new ();
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
            currentEffects = new List<Effect>();
            _serializedObj = null;
            _labelProp = null;
            _descProp = null;
            _iconProp = null;
            _effectsProp = null;
            _tagProp = null;
            _manualProp = null;
        }

        rootVisualElement.Clear();
        CreateGUI();
    }

    [MenuItem("Assets/Open in AbilityView", true)]
    private static bool ValidateOpenAbility()
    {
        return Selection.activeObject is AbilityData;
    }

    [MenuItem("Assets/Open in AbilityView")]
    private static void OpenSelectedAbility()
    {
        if (Selection.activeObject is AbilityData ability)
        {
            Open(ability);
        }
    }

    [MenuItem("Window/UI Toolkit/AbilityView")]
    public static void ShowExample()
    {
        AbilityView wnd = GetWindow<AbilityView>();
        wnd.titleContent = new GUIContent("AbilityView");
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
    public void CreateGUI()
    {
        if(!(currentTags.Keys.Count > 0))
        {
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
        }
        rootVisualElement.Clear();
        GatherTags();

        allEffectTypes = TypeCache.GetTypesDerivedFrom<Effect>()
            .Where(t => !t.IsAbstract && !t.IsGenericType && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name)
            .ToList();

        var root = rootVisualElement;
        root.style.paddingBottom = 6;
        root.style.paddingLeft = 6;
        root.style.paddingRight = 6;
        root.style.paddingTop = 6;

        var infoView = new ScrollView();
        infoView.style.flexGrow = 1;
        root.Add(infoView);

        _label = new TextField("Name");
        _label.style.fontSize = 24;
        _label.style.unityTextAlign = TextAnchor.MiddleCenter;
        _label.value = "Ability Name";
        if(_labelProp != null) _label.BindProperty(_labelProp);
        infoView.Add(_label);

        //Description and Icon View
        var splitViewD = new TwoPaneSplitView(0, 315, TwoPaneSplitViewOrientation.Horizontal);
        splitViewD.style.maxHeight = 128;
        infoView.Add(splitViewD);

        var descLeft = new VisualElement();
        splitViewD.Add(descLeft);
        var descRight = new VisualElement();
        splitViewD.Add(descRight);

        _desc = new TextField("Description");
        _desc.style.whiteSpace = WhiteSpace.Normal;
        _desc.style.flexWrap = Wrap.Wrap;
        _desc.style.paddingRight = 4;
        _desc.style.paddingTop = 4;
        _desc.multiline = true;
        _desc.style.height = 118;
        _desc.value = "Ability Description";
        if(_descProp != null) _desc.BindProperty(_descProp);
        _desc.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

        descLeft.Add(_desc);

        preview = new Image();
        preview.style.width = 96;
        preview.style.height = 96;
        preview.scaleMode = ScaleMode.ScaleToFit;
        preview.style.alignSelf = Align.Center;

        _icon = new UnityEditor.UIElements.ObjectField("Icon")
        {
            objectType = typeof(Sprite),
            allowSceneObjects = false
        };
        if (_iconProp != null) _icon.BindProperty(_iconProp);
        _icon.RegisterValueChangedCallback(evt =>
        {
            var sprite = evt.newValue as Sprite;
            UpdateIconPreview(sprite);
        });

        descRight.Add(preview);
        descRight.Add(_icon);

        //Current Effect List
        effectList = new ScrollView();
        effectList.style.flexDirection = FlexDirection.Column;
        effectList.style.paddingLeft = 4;
        effectList.style.paddingRight = 4;
        effectList.style.paddingTop = 4;
        infoView.Add(effectList);

        for(int i=0; i<currentEffects.Count;i++)
        {
            effectList.Add(CreateEffectItem(currentEffects[i], i));
        }

        //Tags
        var splitViewT = new TwoPaneSplitView(0, 315, TwoPaneSplitViewOrientation.Horizontal);
        splitViewT.style.flexGrow = 1;
        infoView.Add(splitViewT);

        tagLeft = new ScrollView();
        if (currentTags.Keys.Count > 0)
        {
            for (var i = 0; i < currentTags["auto"].Count; i++)
            {
                var tag = currentTags["auto"][i];
                tagLeft.Add(tag.GetVisuals(() => TagView.Open(tag), null, false));
            }
            for (int i = 0; i < currentTags["manual"].Count; i++)
            {
                var tag = currentTags["manual"][i];
                int index = i;
                tagLeft.Add(tag.GetVisuals(() => TagView.Open(tag), () => RemoveTagAt(index), true));
            }
        }
        splitViewT.Add(tagLeft);


        var tagRight = new VisualElement();
        tagRight.style.flexDirection = FlexDirection.Row;
        tagRight.style.flexWrap = Wrap.Wrap;
        tagRight.style.paddingLeft = 4;
        tagRight.style.paddingBottom = 4;
        tagRight.style.paddingTop = 4;
        splitViewT.Add(tagRight);

        foreach(var tag in allTags)
        {
            tagRight.Add(tag.GetVisuals(() => AddTag(tag), null, false));
        }

        //Possible Effects
        var splitViewA = new TwoPaneSplitView(0, 315, TwoPaneSplitViewOrientation.Horizontal);
        splitViewA.style.flexGrow = 1;
        infoView.Add(splitViewA);

        var abilityLeft = new ScrollView();
        splitViewA.Add(abilityLeft);
        abilityRight = new VisualElement();
        splitViewA.Add(abilityRight);

        var grid = new VisualElement();
        grid.style.flexDirection = FlexDirection.Row;
        grid.style.flexWrap = Wrap.Wrap;
        grid.style.paddingLeft = 4;
        grid.style.paddingRight = 4;
        grid.style.paddingTop = 4;
        abilityLeft.Add(grid);

        foreach (var effectType in allEffectTypes)
        {
            grid.Add(CreateEffectTile(effectType));
        }

        abilityRight.Add(new Label("Select an effect to view/edit"));

        //Buttons
        var splitViewB = new TwoPaneSplitView(0, 315, TwoPaneSplitViewOrientation.Horizontal);
        splitViewB.style.maxHeight = 36;
        root.Add(splitViewB);

        var save = new Button(() => SaveInfo());
        save.style.minHeight = 36;
        save.text = "Save";
        splitViewB.Add(save);

        var close = new Button(() => this.Close());
        close.style.minHeight = 36;
        close.text = "Close";
        splitViewB.Add(close);

        //Keep Data Accurate
        RefreshCurrentEffects();
        RefreshCurrentTags();
    }

    private VisualElement CreateEffectTile(Type effectType)
    {
        var button = new Button(() => OnEffectSelected(effectType));
        button.style.width = 88;
        button.style.height = 88;
        button.style.marginRight = 6;
        button.style.marginBottom = 6;
        button.style.alignItems = Align.Center;
        button.style.justifyContent = Justify.Center;
        button.style.flexDirection = FlexDirection.Column;

        var icon = new Image();
        icon.image = GetEffectIcon(effectType);
        icon.scaleMode = ScaleMode.ScaleToFit;
        icon.style.width = 40;
        icon.style.height = 40;
        icon.style.marginBottom = 4;

        var label = new Label(GetEffectDisplayName(effectType));
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.fontSize = 10;

        button.Add(icon);
        button.Add(label);

        return button;
    }
    private void AddTag(CustomTag data)
    {
        if (_obj == null || _serializedObj == null) return;

        _serializedObj.Update();

        var tagProp = _serializedObj.FindProperty("manualTags");

        for(int i = 0; i < tagProp.arraySize; i++)
        {
            var element = tagProp.GetArrayElementAtIndex(i);
            if(element.objectReferenceValue == data)
            {
                return;
            }
        }

        int index = tagProp.arraySize;
        tagProp.InsertArrayElementAtIndex(index);

        var newElement = tagProp.GetArrayElementAtIndex(index);
        newElement.objectReferenceValue = data;

        _serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(_obj);
        AssetDatabase.SaveAssets();

        RefreshCurrentTags();
    }
    private void AddAutoTag(CustomTag data)
    {
        if (_obj == null || _serializedObj == null) return;

        _serializedObj.Update();

        var tagProp = _serializedObj.FindProperty("tagList");

        for (int i = 0; i < tagProp.arraySize; i++)
        {
            var element = tagProp.GetArrayElementAtIndex(i);
            if (element.objectReferenceValue == data)
            {
                return;
            }
        }

        int index = tagProp.arraySize;
        tagProp.InsertArrayElementAtIndex(index);

        var newElement = tagProp.GetArrayElementAtIndex(index);
        newElement.objectReferenceValue = data;

        _serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(_obj);
        AssetDatabase.SaveAssets();

        RefreshCurrentTags();
    }
    private void RefreshCurrentTags()
    {
        tagLeft.Clear();
        foreach(var kvp in currentTags)
        {
            Debug.Log($"Key: {kvp.Key}");
        }
        currentTags["auto"].Clear();
        currentTags["manual"].Clear();

        if (_obj != null)
        {
            foreach (var autoT in _obj.tagList)
            {
                currentTags["auto"].Add(autoT);
            }
            foreach (var manual in _obj.manualTags)
            {
                currentTags["manual"].Add(manual);
            }
        }

        if (currentTags.Keys.Count > 0)
        {
            for (var i = 0; i < currentTags["auto"].Count; i++)
            {
                var tag = currentTags["auto"][i];
                tagLeft.Add(tag.GetVisuals(() => TagView.Open(tag), null, false));
            }
            for (int i = 0; i < currentTags["manual"].Count; i++)
            {
                var tag = currentTags["manual"][i];
                var index = i;
                tagLeft.Add(tag.GetVisuals(() => TagView.Open(tag), () => RemoveTagAt(index), true));
            }
        }
    }
    private void RemoveTagAt(int index)
    {
        Debug.Log($"Index value: {index}");
        _serializedObj.Update();
        _obj.manualTags.RemoveAt(index);
        _serializedObj.ApplyModifiedProperties();

        RefreshCurrentTags();
    }
    private VisualElement CreateEffectItem(Effect effect, int index)
    {
        var container = new VisualElement();
        container.style.height = 36;
        container.style.marginRight = 6;
        container.style.marginBottom = 6;
        container.style.justifyContent = Justify.Center;
        container.style.flexDirection = FlexDirection.Row;

        var label = new Label(effect.DisplayName);
        label.style.fontSize = 16;
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        container.Add(label);

        var info = new Label(effect.Summary);
        info.style.fontSize = 12;
        info.style.unityTextAlign = TextAnchor.MiddleLeft;
        container.Add(info);

        var filler = new VisualElement();
        filler.style.flexGrow = 1;
        container.Add(filler);

        var remove = new Button(() => RemoveEffectAt(index));
        var icon = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
        remove.style.backgroundImage = icon as Texture2D;

        remove.tooltip = "Delete";
        remove.style.height = 24;
        remove.style.width = 24;

        container.Add(remove);

        return container;
    }
    private string GetEffectDisplayName(Type effectType)
    {
        var effect = Activator.CreateInstance(effectType) as Effect;
        return effect?.DisplayName ?? effectType.Name;
    }
    private Texture2D GetEffectIcon(Type effectType)
    {
        var effect = Activator.CreateInstance(effectType) as Effect;
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(effect.IconPath);
        return sprite?.texture ?? null;
    }
    private void OnEffectSelected(Type effectType)
    {
        abilityRight.Clear();

        if (draftHost == null)
            draftHost = ScriptableObject.CreateInstance<EffectHost>();

        draftSerializedObject = new SerializedObject(draftHost);

        draftSerializedObject.Update();

        draftEffectProperty = draftSerializedObject.FindProperty("draftEffect");
        draftEffectProperty.managedReferenceValue = Activator.CreateInstance(effectType);

        draftSerializedObject.ApplyModifiedProperties();

        var title = new Label($"New {effectType.Name}");
        abilityRight.Add(title);

        PopulateEffectVariableFields(draftEffectProperty);
        //var propertyField = new PropertyField(draftEffectProperty);
        //propertyField.BindProperty(draftEffectProperty);
        //abilityRight.Add(propertyField);

        var addButton = new Button(AddEffect)
        {
            text = "Add Effect"
        };
        abilityRight.Add(addButton);
    }
    private void PopulateEffectVariableFields(SerializedProperty draft)
    {
        var copy = draft.Copy();
        var end = copy.GetEndProperty();

        if(!copy.NextVisible(true))
            return;

        while (!SerializedProperty.EqualContents(copy, end))
        {
            var field = new PropertyField(copy.Copy());
            field.Bind(draftSerializedObject);
            abilityRight.Add(field);
            copy.NextVisible(false);
        }
    }
    private void AddEffect()
    {
        if(_obj == null || _serializedObj == null || draftEffectProperty == null)
        return;

        _serializedObj.Update();

        var effectsProp = _serializedObj.FindProperty("effects");

        int index = effectsProp.arraySize;
        effectsProp.InsertArrayElementAtIndex(index);

        var newElement = effectsProp.GetArrayElementAtIndex(index);

        // Copy the configured draft object into the real ability list
        newElement.managedReferenceValue = draftEffectProperty.managedReferenceValue;

        _serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(_obj);
        AssetDatabase.SaveAssets();

        RefreshCurrentEffects();
        ClearEffectDetails();
    }
    private void RemoveEffectAt(int index)
    {
        _serializedObj.Update();
        _obj.effects.RemoveAt(index);
        _serializedObj.ApplyModifiedProperties();

        RefreshCurrentEffects();
        ClearEffectDetails();
    }
    private void RefreshCurrentEffects()
    {
        effectList.Clear();

        if (_obj != null)
            currentEffects = _obj.effects;

        for(int i=0; i< currentEffects.Count; i++)
        {
            foreach (var t in currentEffects[i].tags)
                AddAutoTag(t);
            effectList.Add(CreateEffectItem(currentEffects[i], i));
        }
    }
    private void ClearEffectDetails()
    {
        abilityRight.Clear();
        abilityRight.Add(new Label("Select an effect to view/edit"));
    }
    private void UpdateIconPreview(Sprite sprite)
    {
        preview.image = sprite != null ? sprite.texture : null;
    }
    private void SaveInfo()
    {
        var formatID = _label.value;
        formatID = formatID.Replace(" ", "_");
        formatID = formatID.ToLower();

        if (_obj == null || _serializedObj == null)
        {
            var ability = ScriptableObject.CreateInstance<AbilityData>();
            ability.id = "ability_" + formatID;
            ability.label = _label.value;
            ability.desc = _desc.value;
            ability.icon = _icon.value as Sprite;
            ability.effects = currentEffects;

            AssetDatabase.CreateAsset(ability, "Assets/Data/Abilities/" + _label.value + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SetAbility(ability);
        }
        else
        {
            _serializedObj.Update();

            var id = _serializedObj.FindProperty("id");
            id.stringValue = "ability_" + formatID;

            _serializedObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(_obj);
            AssetDatabase.SaveAssets();
        }
    }
}