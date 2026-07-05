using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
public class MonsterView : EditorWindow
{
    private List<CustomTag> allTags;
    private List<AbilityData> allAbilities;
    private List<AbilityData> currentAbilities = new();
    private Dictionary<string, List<CustomTag>> currentTags = new();
    private LootTableData loot;

    private ScrollView abilityList;
    private ScrollView currentAbilityList;
    private ScrollView tagLeft;
    private VisualElement lootView;
    private TextField _label;
    private TextField _desc;
    private Image _preview;
    private UnityEditor.UIElements.ObjectField _icon;

    private MonsterData _obj;
    private SerializedObject _serializedObj;
    private SerializedProperty _labelProp;
    private SerializedProperty _descProp;
    private SerializedProperty _iconProp;
    private SerializedProperty _abilitiesProp;
    private SerializedProperty _tagProp;
    private SerializedProperty _lootProp;

    private void OnSelectionChange()
    {
        if (Selection.activeObject is MonsterData data)
        {
            SetData(data);
        }
    }
    private void OnFocus()
    {
        rootVisualElement.Clear();
        if (_obj == null || _serializedObj == null)
        {
            currentTags = new();
            currentAbilities = new();
        }
        CreateGUI();
    }
    public static void Open(MonsterData data)
    {
        var window = GetWindow<MonsterView>("MonsterView");
        window.Focus();
        window.SetData(data);
    }
    private void SetData(MonsterData data)
    {
        _obj = data;

        if (_obj != null)
        {
            currentTags = new();
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
            foreach (var auto in _obj.tagList)
            {
                currentTags["auto"].Add(auto);
            }
            foreach (var manual in _obj.manualTags)
            {
                currentTags["manual"].Add(manual);
            }
            currentAbilities = _obj.abilities ?? new List<AbilityData>();
            loot = _obj.lootTable ?? ScriptableObject.CreateInstance<LootTableData>();
            _serializedObj = new SerializedObject(_obj);
            _labelProp = _serializedObj.FindProperty("label");
            _descProp = _serializedObj.FindProperty("desc");
            _iconProp = _serializedObj.FindProperty("icon");
            _abilitiesProp = _serializedObj.FindProperty("abilities");
            _tagProp = _serializedObj.FindProperty("tagList");
            _lootProp = _serializedObj.FindProperty("lootTable");
        }
        else
        {
            currentTags = new();
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
            currentAbilities = new List<AbilityData>();
            loot = ScriptableObject.CreateInstance<LootTableData>();
            _serializedObj = null;
            _labelProp = null;
            _descProp = null;
            _iconProp = null;
            _abilitiesProp = null;
            _tagProp = null;
            _lootProp = null;

            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/Loot Tables/lootTable_.asset");
            AssetDatabase.CreateAsset(loot, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        rootVisualElement.Clear();
        CreateGUI();
    }

    [MenuItem("Assets/Open in MonsterView", true)]
    private static bool ValidateOpenMonster()
    {
        return Selection.activeObject is MonsterData;
    }

    [MenuItem("Assets/Open in MonsterView")]
    private static void OpenSelectedMonster()
    {
        if (Selection.activeObject is MonsterData monster)
        {
            Open(monster);
        }
    }
    [MenuItem("Window/UI Toolkit/MonsterView")]
    public static void ShowExample()
    {
        MonsterView wnd = GetWindow<MonsterView>();
        wnd.titleContent = new GUIContent("MonsterView");
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
        if (!(currentTags.Keys.Count > 0))
        {
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
        }
        GatherTags();
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

        rootVisualElement.Clear();
        VisualElement root = rootVisualElement;

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
        _label.value = "Monster Name";
        if (_labelProp != null) _label.BindProperty(_labelProp);
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
        _desc.value = "Monster Description";
        if (_descProp != null) _desc.BindProperty(_descProp);
        _desc.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

        descLeft.Add(_desc);

        _preview = new Image();
        _preview.style.width = 96;
        _preview.style.height = 96;
        _preview.scaleMode = ScaleMode.ScaleToFit;
        _preview.style.alignSelf = Align.Center;

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

        descRight.Add(_preview);
        descRight.Add(_icon);

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

        foreach (var tag in allTags)
        {
            tagRight.Add(tag.GetVisuals(() => AddTag(tag), null, false));
        }

        //Loot Table
        lootView = loot.GetVisuals(() => LootEntryView.Open(loot));
        infoView.Add(lootView);

        var listView = new ScrollView();
        listView.style.flexGrow = 1;
        lootView.Add(listView);
        foreach (var item in loot.table)
        {
            lootView.Add(item.GetVisuals(() => ItemView.Open(item.item)));
        }

        //Current Ability List
        var currentLabel = new Label();
        currentLabel.text = "Known Abilities";
        currentLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        currentLabel.style.fontSize = 18;
        infoView.Add(currentLabel);

        currentAbilityList = new ScrollView();
        currentAbilityList.contentContainer.style.flexDirection = FlexDirection.Row;
        currentAbilityList.contentContainer.style.flexWrap = Wrap.Wrap;
        currentAbilityList.contentContainer.style.paddingLeft = 4;
        currentAbilityList.contentContainer.style.paddingRight = 4;
        currentAbilityList.contentContainer.style.paddingTop = 4;
        infoView.Add(currentAbilityList);

        for (int i = 0; i < currentAbilities.Count; i++)
        {
            currentAbilityList.Add(CreateAbilityList(currentAbilities[i], i));
        }

        if(currentAbilities.Count < 1)
        {
            currentAbilityList.Add(new Label("None"));
        }

        //All Abilities View
        var allAbilitiesLabel = new Label();
        allAbilitiesLabel.text = "All Abilities";
        allAbilitiesLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        allAbilitiesLabel.style.fontSize = 18;
        infoView.Add(allAbilitiesLabel);

        abilityList = new ScrollView();
        abilityList.contentContainer.style.flexDirection = FlexDirection.Row;
        abilityList.contentContainer.style.flexWrap = Wrap.Wrap;
        abilityList.contentContainer.style.paddingLeft = 4;
        abilityList.contentContainer.style.paddingRight = 4;
        abilityList.contentContainer.style.paddingTop = 4;
        infoView.Add(abilityList);

        PopulateAbilities(abilityList);

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
    }
    private void AddTag(CustomTag data)
    {
        if (_obj == null || _serializedObj == null) return;

        _serializedObj.Update();

        var tagProp = _serializedObj.FindProperty("manualTags");

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
    private void RemoveTagAt(int index)
    {
        _serializedObj.Update();
        _obj.tagList.RemoveAt(index);
        _serializedObj.ApplyModifiedProperties();

        RefreshCurrentTags();
    }
    private void RefreshCurrentTags()
    {
        tagLeft.Clear();
        foreach (var kvp in currentTags)
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
        _serializedObj.Update();
        _obj.abilities.RemoveAt(index);
        _serializedObj.ApplyModifiedProperties();

        RefreshCurrentAbilities();
    }
    private void RefreshCurrentAbilities()
    {
        currentAbilityList.Clear();

        if (_obj != null)
            currentAbilities = _obj.abilities;

        for (int i = 0; i < currentAbilities.Count; i++)
        {
            currentAbilityList.Add(CreateAbilityList(currentAbilities[i], i));
        }
    }
    private void UpdateIconPreview(Sprite sprite)
    {
        _preview.image = sprite != null ? sprite.texture : null;
    }
    private void AddAbility(AbilityData data)
    {
        if (_obj == null || _serializedObj == null || data == null)
            return;

        _serializedObj.Update();

        int index = _abilitiesProp.arraySize;
        _abilitiesProp.InsertArrayElementAtIndex(index);

        var newElement = _abilitiesProp.GetArrayElementAtIndex(index);

        // Copy the configured draft object into the real ability list
        newElement.objectReferenceValue = data;

        _serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(_obj);
        AssetDatabase.SaveAssets();

        RefreshCurrentAbilities();
    }
    private void SaveInfo()
    {
        var formatID = _label.value;
        formatID = formatID.Replace(" ", "_");
        formatID = formatID.ToLower();

        if (_obj == null || _serializedObj == null)
        {
            var data = ScriptableObject.CreateInstance<MonsterData>();
            data.id = "monster_" + formatID;
            data.label = _label.value;
            data.desc = _desc.value;
            data.icon = _icon.value as Sprite;
            data.abilities = currentAbilities;
            data.lootTable = loot;

            data.lootTable.id = $"lootTable_{formatID}";
            data.lootTable.label = $"Loot Table ({_label.value})";

            AssetDatabase.CreateAsset(data, "Assets/Data/Monsters/" + _label.value + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SetData(data);
        }
        else
        {
            _serializedObj.Update();

            var id = _serializedObj.FindProperty("id");
            id.stringValue = "monster_" + formatID;

            string path = AssetDatabase.GetAssetPath(loot);
            Debug.Log($"OldPath: {path}");
            string newPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(Path.GetDirectoryName(path), $"lootTable_{formatID}.asset")
            );
            Debug.Log($"NewPath: {newPath}");

            string uniqueName = Path.GetFileNameWithoutExtension(newPath);
            Debug.Log($"UniqueName: {uniqueName}");
            AssetDatabase.RenameAsset(path,uniqueName);

            _serializedObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(_obj);
            AssetDatabase.SaveAssets();
        }
    }
}