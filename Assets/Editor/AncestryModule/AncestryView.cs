using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class AncestryView : EditorWindow
{
    private Dictionary<string, List<CustomTag>> currentTags = new();
    private Image preview;

    private List<DisplayerCore> displayers = new();
    private List<EntityModule> currentModules = new();
    private SerializedProperty _modProp;

    private TextField _label;
    private TextField _desc;
    private Image _preview;
    private UnityEditor.UIElements.ObjectField _icon;

    private AncestryData _obj;
    private SerializedObject _serializedObj;
    private SerializedProperty _labelProp;
    private SerializedProperty _descProp;
    private SerializedProperty _mechanicProp;
    private SerializedProperty _iconProp;


    private void OnSelectionChange()
    {
        if (Selection.activeObject is AncestryData data)
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
    public static void Open(AncestryData data)
    {
        var window = GetWindow<AncestryView>("AncestryView");
        window.Focus();
        window.SetData(data);
    }
    private void SetData(AncestryData data)
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

            currentModules = _obj.modules ?? new List<EntityModule>();

            displayers.Clear();
            // Core module
            if (_obj != null)
                displayers.Add(new TagDisplayer(_obj.tags));

            // Optional modules
            foreach (EntityModule module in currentModules)
            {
                displayers.Add(DisplayerCore.GetDisplayer(module));
            }

            _serializedObj = new SerializedObject(_obj);
            _modProp = _serializedObj.FindProperty("modules");
            _labelProp = _serializedObj.FindProperty("label");
            _descProp = _serializedObj.FindProperty("description");
            _iconProp = _serializedObj.FindProperty("icon");
            _mechanicProp = _serializedObj.FindProperty("mechanics");
        }
        else
        {
            currentTags = new();
            currentTags.Add("auto", new());
            currentTags.Add("manual", new());
            currentModules = new();
            _modProp = null;

            _labelProp = null;
            _descProp = null;
            _iconProp = null;
            _mechanicProp = null;
        }

        rootVisualElement.Clear();
        CreateGUI();
    }

    [MenuItem("Assets/Crying Forest/Open in AncestryView", true)]
    private static bool ValidateOpen()
    {
        return Selection.activeObject is AncestryData;
    }

    [MenuItem("Assets/Crying Forest/Open in AncestryView")]
    private static void OpenSelected()
    {
        if (Selection.activeObject is AncestryData actor)
        {
            Open(actor);
        }
    }
    [MenuItem("Window/Crying Forest Toolkit/AncestryView")]
    public static void ShowAncestry()
    {
        AncestryView wnd = GetWindow<AncestryView>();
        wnd.titleContent = new GUIContent("AncestryView");
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

        // Label Section
        _label = new TextField("Name");
        _label.style.fontSize = 24;
        _label.style.unityTextAlign = TextAnchor.MiddleCenter;
        _label.value = "Ancestry Name";
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
        _desc.value = "Ability Description";
        if (_descProp != null) _desc.BindProperty(_descProp);
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

        /*
         * 
         * Place your specific views and variables you want to adjust here
         * 
        */

        // Displayers
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

    //Module Functions
    private List<Type> GetAvailableModuleTypes()
    {
        return TypeCache.GetTypesDerivedFrom<EntityModule>()
        .Where(type =>
            !type.IsAbstract &&
            !type.IsGenericType &&
            type != typeof(TagModule) &&
            type.GetConstructor(Type.EmptyTypes) != null)
        .Union(new[] { typeof(MechanicModule) })
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
                else if (GetModuleDisplayName(capturedType) == "Example")
                {
                    continue;
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
        if(_obj != null)
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
            //Enter your specific serialization here
            var data = ScriptableObject.CreateInstance<AncestryData>();
            data.id = "ancestry_" + formatID;
            data.label = _label.value;
            data.modules = currentModules;

            //Set where to save your assets to, make sure to update the path appropriately
            string path = AssetDatabase.GenerateUniqueAssetPath(
                "Assets/Data/Ancestries/" + _label.value + ".asset"
            );

            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SetData(data);
        }
        else
        {
            _serializedObj.Update();

            var id = _serializedObj.FindProperty("id");
            id.stringValue = "ancestry_" + formatID;

            _serializedObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(_obj);
            AssetDatabase.SaveAssets();
        }
    }
}