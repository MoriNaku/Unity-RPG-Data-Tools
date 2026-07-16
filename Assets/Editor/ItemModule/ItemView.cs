using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
public class ItemView : EditorWindow
{
    private List<Type> allEffectTypes;
    private ScrollView effectList;
    private ScrollView currentEffectList;
    private VisualElement abilityRight;

    private List<DisplayerCore> displayers = new();
    private List<EntityModule> currentModules = new();
    private SerializedProperty _modProp;

    private ItemData _obj;
    private SerializedObject _serializedObj;
    private SerializedProperty _labelProp;
    private SerializedProperty _descProp;
    private SerializedProperty _iconProp;
    private SerializedProperty _effectProp;
    private SerializedProperty _typeProp;
    private SerializedProperty _weaponProp;
    private SerializedProperty _equipProp;
    private SerializedProperty _stackProp;
    private SerializedProperty _reuseProp;
    private SerializedProperty _tagProp;

    private TextField _label;
    private TextField _desc;
    private Image _preview;
    private UnityEditor.UIElements.ObjectField _icon;
    private List<Effect> currentEffects = new();
    private Dictionary<string, List<CustomTag>> currentTags = new();
    private IntegerField maxStack;
    private Toggle isReuse;
    private EnumField _itemType;
    private EnumField _weaponType;
    private EnumField _equipType;

    private EffectHost draftHost;
    private SerializedObject draftSerializedObject;
    private SerializedProperty draftEffectProperty;

    private void OnSelectionChange()
    {
        if (Selection.activeObject is ItemData data)
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
            currentEffects = new();
        }
        CreateGUI();
    }
    public static void Open(ItemData data)
    {
        var window = GetWindow<ItemView>("ItemView");
        window.Focus();
        window.SetData(data);
    }
    private void SetData(ItemData data)
    {
        displayers.Clear();
        displayers.Add(new TagDisplayer(new TagModule()));
        _obj = data;

        if (_obj != null)
        {
            currentTags = new();
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
            foreach (var auto in _obj.tags.auto)
            {
                currentTags["auto"].Add(auto);
            }
            foreach (var manual in _obj.tags.manual)
            {
                currentTags["manual"].Add(manual);
            }

            displayers.Clear();
            displayers.Add(new TagDisplayer(_obj.tags));
            foreach (EntityModule e in _obj.modules)
            {
                displayers.Add(DisplayerCore.GetDisplayer(e));
            }
            currentModules = _obj.modules ?? new List<EntityModule>();

            currentEffects = _obj.effects ?? new List<Effect>();
            _serializedObj = new SerializedObject(_obj);
            _labelProp = _serializedObj.FindProperty("label");
            _descProp = _serializedObj.FindProperty("desc");
            _iconProp = _serializedObj.FindProperty("icon");
            _effectProp = _serializedObj.FindProperty("effects");
            _typeProp = _serializedObj.FindProperty("itemType");
            _weaponProp = _serializedObj.FindProperty("weaponType");
            _equipProp = _serializedObj.FindProperty("equipType");
            _stackProp = _serializedObj.FindProperty("maxStack");
            _reuseProp = _serializedObj.FindProperty("isReuseable");
            _tagProp = _serializedObj.FindProperty("tagList");
            _modProp = _serializedObj.FindProperty("modules");
        }
        else
        {
            currentTags = new();
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
            currentEffects = new List<Effect>();
            _serializedObj = null;
            _labelProp = null;
            _descProp = null;
            _iconProp = null;
            _effectProp = null;
            _typeProp = null;
            _weaponProp = null;
            _equipProp = null;
            _stackProp = null;
            _reuseProp = null;
            _tagProp = null;
            _modProp = null;
        }

        rootVisualElement.Clear();
        CreateGUI();
    }

    [MenuItem("Assets/Crying Forest/Open in ItemView", true)]
    private static bool ValidateOpen()
    {
        return Selection.activeObject is ItemData;
    }

    [MenuItem("Assets/Crying Forest/Open in ItemView")]
    private static void OpenSelected()
    {
        if (Selection.activeObject is ItemData data)
        {
            Open(data);
        }
    }
    [MenuItem("Window/Crying Forest Toolkit/ItemView")]
    public static void ShowExample()
    {
        ItemView wnd = GetWindow<ItemView>();
        wnd.titleContent = new GUIContent("ItemView");
    }
    public void CreateGUI()
    {
        allEffectTypes = TypeCache.GetTypesDerivedFrom<Effect>()
            .Where(t => !t.IsAbstract && !t.IsGenericType && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.Name)
            .ToList();

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
        _label.value = "Item Name";
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
        _desc.style.height = 52;
        _desc.value = "Item Description";
        if (_descProp != null) _desc.BindProperty(_descProp);
        _desc.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

        _itemType = new EnumField("Item Type", ItemType.Key);
        _itemType.style.paddingRight = 4;
        _itemType.style.paddingTop = 4;
        if (_typeProp != null) _itemType.BindProperty(_typeProp);
        descLeft.Add(_itemType);

        //Consumable Section
        var consumablePane = new VisualElement();
        consumablePane.style.paddingTop = 4;
        consumablePane.style.paddingRight = 4;

        maxStack = new IntegerField("Maximum Stackable");
        if (_stackProp != null) maxStack.BindProperty(_stackProp);

        isReuse = new Toggle("Reusable?");
        isReuse.style.paddingTop = 4;
        if (_reuseProp != null) isReuse.BindProperty(_reuseProp);

        consumablePane.Add(maxStack);
        consumablePane.Add(isReuse);
        descLeft.Add(consumablePane);

        //Equip Section
        var equipPane = new VisualElement();
        equipPane.style.paddingRight = 4;
        equipPane.style.paddingTop = 4;

        _weaponType = new EnumField("Weapon Type", WeaponType.Sword);
        _weaponType.style.paddingTop = 4;
        if (_weaponProp != null) _weaponType.BindProperty(_weaponProp);

        _equipType = new EnumField("Equipment Type", EquipType.Weapon);
        if (_equipProp != null) _equipType.BindProperty(_equipProp);

        equipPane.Add(_equipType);
        equipPane.Add(_weaponType);
        descLeft.Add(equipPane);

        void RefreshSections()
        {
            var selectedI = (ItemType)_itemType.value;

            consumablePane.style.display = selectedI == ItemType.Consumable ? DisplayStyle.Flex : DisplayStyle.None;

            equipPane.style.display = selectedI == ItemType.Equip ? DisplayStyle.Flex : DisplayStyle.None;

            var selectedE = (EquipType)_equipType.value;

            _weaponType.style.display = selectedE == EquipType.Weapon ? DisplayStyle.Flex : DisplayStyle.None;

            if (selectedI == ItemType.Key)
            {
                _desc.style.height = 122;
            } else if (selectedI == ItemType.Consumable)
            {
                _desc.style.height = 52;
            } else if (selectedI == ItemType.Equip)
            {
                if (selectedE == EquipType.Weapon)
                {
                    _desc.style.height = 52;
                }
                else
                {
                    _desc.style.height = 72;
                }
            }
        }

        _itemType.RegisterValueChangedCallback(_ => RefreshSections());
        _equipType.RegisterValueChangedCallback(_ => RefreshSections());

        RefreshSections();
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

        //Displayers
        foreach (var d in displayers)
        {
            VisualElement moduleView = d.CraftView();

            if (d.Module != null &&
                d.Module is not TagModule)
            {
                var removeButton = new Button(
                    () => RemoveModule(d.Module))
                {
                    text = $"Remove {GetModuleDisplayName(d.Module.GetType())} Module"
                };

                moduleView.Add(removeButton);
            }

            infoView.Add(moduleView);
        }

        //Current Effect List
        effectList = new ScrollView();
        effectList.style.flexDirection = FlexDirection.Column;
        effectList.style.paddingLeft = 4;
        effectList.style.paddingRight = 4;
        effectList.style.paddingTop = 4;
        infoView.Add(effectList);

        for (int i = 0; i < currentEffects.Count; i++)
        {
            effectList.Add(CreateListItem(currentEffects[i].DisplayName, currentEffects[i].Summary, i));
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
            var e = Activator.CreateInstance(effectType) as Effect;
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(e.IconPath);
            grid.Add(CreateIconTile(s, e.DisplayName, () => OnEffectSelected(effectType)));
        }

        abilityRight.Add(new Label("Select an effect to view/edit"));

        infoView.Add(CreateAddModuleButton());

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
    private VisualElement CreateIconTile(Sprite sprite, string labelText, Action onClick, string tooltip = null)
    {
        var button = new Button(() => onClick?.Invoke());
        button.style.width = 88;
        button.style.height = 88;
        button.style.marginRight = 6;
        button.style.marginBottom = 6;
        button.style.alignItems = Align.Center;
        button.style.justifyContent = Justify.Center;
        button.style.flexDirection = FlexDirection.Column;

        var icon = new Image();
        if (sprite != null) icon.image = sprite.texture;
        icon.scaleMode = ScaleMode.ScaleToFit;
        icon.style.width = 40;
        icon.style.height = 40;
        icon.style.marginBottom = 4;

        var label = new Label(labelText ?? "");
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.fontSize = 10;

        button.Add(icon);
        button.Add(label);

        return button;
    }
    private VisualElement CreateListItem(string name, string desc, int index)
    {
        var container = new VisualElement();
        container.style.height = 36;
        container.style.marginRight = 6;
        container.style.marginBottom = 6;
        container.style.justifyContent = Justify.Center;
        container.style.flexDirection = FlexDirection.Row;

        var label = new Label(name);
        label.style.fontSize = 16;
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        container.Add(label);

        var info = new Label(desc);
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

        if (!copy.NextVisible(true))
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
        if (_obj == null || _serializedObj == null || draftEffectProperty == null)
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

        for (int i = 0; i < currentEffects.Count; i++)
        {
            effectList.Add(CreateListItem(currentEffects[i].DisplayName, currentEffects[i].Summary, i));
        }
    }
    private void ClearEffectDetails()
    {
        abilityRight.Clear();
        abilityRight.Add(new Label("Select an effect to view/edit"));
    }
    private void UpdateIconPreview(Sprite sprite)
    {
        _preview.image = sprite != null ? sprite.texture : null;
    }

    //Module Functions
    private List<Type> GetAvailableModuleTypes()
    {
        return TypeCache.GetTypesDerivedFrom<EntityModule>()
        .Where(type =>
            !type.IsAbstract &&
            !type.IsGenericType &&
            type != typeof(TagModule) &&
            type.GetConstructor(Type.EmptyTypes) != null)
        .OrderBy(type => type.Name)
        .ToList();
    }
    private string GetModuleDisplayName(Type moduleType)
    {
        string name = moduleType.Name;

        if (name.EndsWith("Module"))
            name = name[..^"Module".Length];

        return ObjectNames.NicifyVariableName(name);
    }
    private VisualElement CreateAddModuleButton()
    {
        var button = new Button
        {
            text = "Add Module"
        };

        button.clicked += () =>
        {
            GenericMenu menu = new GenericMenu();

            foreach (Type moduleType in GetAvailableModuleTypes())
            {

                Type capturedType = moduleType;
                bool alreadyAdded = HasModule(capturedType);

                if (alreadyAdded)
                {
                    menu.AddDisabledItem(
                        new GUIContent(GetModuleDisplayName(capturedType)));
                }
                else
                {
                    menu.AddItem(
                        new GUIContent(GetModuleDisplayName(capturedType)),
                        false,
                        () => AddModule(capturedType));
                }
            }

            menu.ShowAsContext();
        };
        button.SetEnabled(_obj != null);

        return button;
    }
    private void AddModule(Type moduleType)
    {
        if (_obj == null || _serializedObj == null)
            return;

        _serializedObj.Update();

        if (_modProp == null)
        {
            Debug.LogError(
                $"Could not find the modules property on {_obj.name}.");

            return;
        }

        if (HasModule(moduleType))
            return;

        int index = _modProp.arraySize;
        _modProp.InsertArrayElementAtIndex(index);

        SerializedProperty newModule =
            _modProp.GetArrayElementAtIndex(index);

        newModule.managedReferenceValue =
            Activator.CreateInstance(moduleType);

        _serializedObj.ApplyModifiedProperties();

        EditorUtility.SetDirty(_obj);
        AssetDatabase.SaveAssets();

        RefreshDisplayers();
    }
    private bool HasModule(Type moduleType)
    {
        return _obj.modules.Any(m => m.GetType() == moduleType);
    }
    private void RefreshDisplayers()
    {
        displayers.Clear();

        // Core module
        if (_obj != null)
            displayers.Add(new TagDisplayer(_obj.tags));

        // Optional modules
        foreach (EntityModule module in currentModules)
        {
            displayers.Add(DisplayerCore.GetDisplayer(module));
        }

        rootVisualElement.Clear();
        CreateGUI();
    }
    private void RemoveModule(EntityModule module)
    {
        if (!EditorUtility.DisplayDialog(
            "Remove Module",
            $"Are you sure you want to remove the {GetModuleDisplayName(module.GetType())} module?\n\n{module.RemoveWarning}",
            "Remove",
            "Cancel"))
        {
            return;
        }

        _serializedObj.Update();

        for (int i = 0; i < _modProp.arraySize; i++)
        {
            SerializedProperty element = _modProp.GetArrayElementAtIndex(i);

            if (ReferenceEquals(element.managedReferenceValue, module))
            {
                _modProp.DeleteArrayElementAtIndex(i);
                break;
            }
        }
        currentModules.Remove(module);

        _serializedObj.ApplyModifiedProperties();

        EditorUtility.SetDirty(_obj);
        AssetDatabase.SaveAssets();

        RefreshDisplayers();
    }

    private void SaveInfo()
    {
        var formatID = _label.value;
        formatID = formatID.Replace(" ", "_");
        formatID = formatID.ToLower();

        if ((ItemType)_itemType.value != ItemType.Equip)
        {
            formatID = "item_" + formatID;
        }
        else
        {
            if ((EquipType)_equipType.value != EquipType.Weapon)
            {
                formatID = "armor_" + formatID;
            }
            else
            {
                formatID = "weapon_" + formatID;
            }
        }

        if (_obj == null || _serializedObj == null)
        {
            if ((ItemType)_itemType.value != ItemType.Equip)
            {
                var data = ScriptableObject.CreateInstance<ConsumableData>();
                data.id = formatID;
                data.label = _label.value;
                data.desc = _desc.value;
                data.icon = _icon.value as Sprite;
                data.effects = currentEffects;
                data.itemType = (ItemType)_itemType.value;
                data.maxStack = maxStack.value;
                data.isReuseable = isReuse.value;
                data.modules = currentModules;
                
                AssetDatabase.CreateAsset(data, "Assets/Data/Items/" + _label.value + ".asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                SetData(data);
            }
            else
            {
                if((EquipType)_equipType.value != EquipType.Weapon)
                {
                    var data = ScriptableObject.CreateInstance<ArmorData>();
                    data.id = formatID;
                    data.label = _label.value;
                    data.desc = _desc.value;
                    data.icon = _icon.value as Sprite;
                    data.effects = currentEffects;
                    data.itemType = (ItemType)_itemType.value;
                    data.equipType = (EquipType)_equipType.value;
                    data.modules = currentModules;

                    AssetDatabase.CreateAsset(data, "Assets/Data/Items/" + _label.value + ".asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    SetData(data);
                }
                else
                {
                    var data = ScriptableObject.CreateInstance<WeaponData>();
                    data.id = formatID;
                    data.label = _label.value;
                    data.desc = _desc.value;
                    data.icon = _icon.value as Sprite;
                    data.effects = currentEffects;
                    data.itemType = (ItemType)_itemType.value;
                    data.equipType = (EquipType)_equipType.value;
                    data.weaponType = (WeaponType)_weaponType.value;
                    data.modules = currentModules;

                    AssetDatabase.CreateAsset(data, "Assets/Data/Items/" + _label.value + ".asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    SetData(data);
                }
            }
        }
        else
        {
            _serializedObj.Update();

            var id = _serializedObj.FindProperty("id");
            id.stringValue = formatID;

            _serializedObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(_obj);
            AssetDatabase.SaveAssets();
        }
    }
}