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
public class ActorView : EditorWindow
{
    private Dictionary<string, List<CustomTag>> currentTags = new();
    private LootTableData loot;

    private List<DisplayerCore> displayers = new();
    private List<EntityModule> currentModules = new();
    private SerializedProperty _modProp;

    private VisualElement lootView;
    private TextField _label;
    private TextField _desc;
    private Image _preview;
    private UnityEditor.UIElements.ObjectField _icon;

    private ActorData _obj;
    private SerializedObject _serializedObj;
    private SerializedProperty _labelProp;
    private SerializedProperty _descProp;
    private SerializedProperty _iconProp;
    private SerializedProperty _tagProp;
    private SerializedProperty _lootProp;

    
    private void OnSelectionChange()
    {
        if (Selection.activeObject is ActorData data)
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
            currentModules = new();
        }
        RefreshDisplayers();
        CreateGUI();
    }
    public static void Open(ActorData data)
    {
        var window = GetWindow<ActorView>("ActorView");
        window.Focus();
        window.SetData(data);
    }
    private void SetData(ActorData data)
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

            loot = _obj.lootTable ?? ScriptableObject.CreateInstance<LootTableData>();
            _serializedObj = new SerializedObject(_obj);
            _labelProp = _serializedObj.FindProperty("label");
            _descProp = _serializedObj.FindProperty("desc");
            _iconProp = _serializedObj.FindProperty("icon");
            _tagProp = _serializedObj.FindProperty("tags");
            _lootProp = _serializedObj.FindProperty("lootTable");
            _modProp = _serializedObj.FindProperty("modules");
        }
        else
        {
            currentTags = new();
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
            currentModules = new();
            loot = ScriptableObject.CreateInstance<LootTableData>();
            _serializedObj = null;
            _labelProp = null;
            _descProp = null;
            _iconProp = null;
            _tagProp = null;
            _lootProp = null;
            _modProp = null;

            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/Data/Loot Tables/lootTable_.asset");
            AssetDatabase.CreateAsset(loot, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        rootVisualElement.Clear();
        CreateGUI();
    }

    [MenuItem("Assets/Open in ActorView", true)]
    private static bool ValidateOpenActor()
    {
        return Selection.activeObject is ActorData;
    }

    [MenuItem("Assets/Open in ActorView")]
    private static void OpenSelectedActor()
    {
        if (Selection.activeObject is ActorData actor)
        {
            Open(actor);
        }
    }
    [MenuItem("Window/UI Toolkit/ActorView")]
    public static void ShowExample()
    {
        ActorView wnd = GetWindow<ActorView>();
        wnd.titleContent = new GUIContent("ActorView");
    }
    public void CreateGUI()
    {
        if (!(currentTags.Keys.Count > 0))
        {
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
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
        _label.value = "Actor Name";
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
        _desc.value = "Actor Description";
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

        // Displayers
        foreach(var d in displayers)
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

        //Loot Table
        if (loot == null)
        {
            loot = ScriptableObject.CreateInstance<LootTableData>();
        }
        lootView = loot.GetVisuals(() => LootEntryView.Open(loot));
        infoView.Add(lootView);

        var listView = new ScrollView();
        listView.style.flexGrow = 1;
        lootView.Add(listView);
        foreach (var item in loot.table)
        {
            lootView.Add(item.GetVisuals(() => ItemView.Open(item.item)));
        }

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
        displayers.Add(new TagDisplayer(_obj.tags));

        // Optional modules
        foreach (EntityModule module in currentModules)
        {
            Debug.Log($"Module Name: {module.GetType().FullName}");
            displayers.Add(DisplayerCore.GetDisplayer(module));
        }

        foreach(var d in displayers)
        {
            Debug.Log($"Displayer Name: {d.GetType().FullName}");
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

        if (_obj == null || _serializedObj == null)
        {
            var data = ScriptableObject.CreateInstance<ActorData>();
            data.id = "actor_" + formatID;
            data.label = _label.value;
            data.desc = _desc.value;
            data.icon = _icon.value as Sprite;
            //data.abilities = currentAbilities;
            data.lootTable = loot;
            data.modules = currentModules;

            data.lootTable.id = $"lootTable_{formatID}";
            data.lootTable.label = $"Loot Table ({_label.value})";

            AssetDatabase.CreateAsset(data, "Assets/Data/Actors/" + _label.value + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SetData(data);
        }
        else
        {
            _serializedObj.Update();

            var id = _serializedObj.FindProperty("id");
            id.stringValue = "actor_" + formatID;

            string path = AssetDatabase.GetAssetPath(loot);
            //Debug.Log($"OldPath: {path}");
            string newPath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(Path.GetDirectoryName(path), $"lootTable_{formatID}.asset")
            );
            //Debug.Log($"NewPath: {newPath}");

            string uniqueName = Path.GetFileNameWithoutExtension(newPath);
            Debug.Log($"UniqueName: {uniqueName}");
            AssetDatabase.RenameAsset(path, uniqueName);

            _serializedObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(_obj);
            AssetDatabase.SaveAssets();
        }
    }
}