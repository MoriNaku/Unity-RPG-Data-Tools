using Codice.Client.BaseCommands.Merge.Xml;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class TagView : EditorWindow
{
    private CustomTag _obj;
    private SerializedObject _serializedObj;
    private SerializedProperty _labelProp;
    private SerializedProperty _iconProp;
    private SerializedProperty _typeProp;

    private TextField _label;
    private Image _preview;
    private UnityEditor.UIElements.ObjectField _icon;
    private EnumField _tagType;
    private void OnSelectionChange()
    {
        if (Selection.activeObject is CustomTag data)
        {
            SetData(data);
        }
    }
    private void OnFocus()
    {
        rootVisualElement.Clear();
        CreateGUI();
    }
    public static void Open(CustomTag data)
    {
        var window = GetWindow<TagView>("TagView");
        window.Focus();
        window.SetData(data);
    }
    private void SetData(CustomTag data)
    {
        _obj = data;

        if (_obj != null)
        {
            _serializedObj = new SerializedObject(_obj);
            _labelProp = _serializedObj.FindProperty("label");
            _iconProp = _serializedObj.FindProperty("icon");
            _typeProp = _serializedObj.FindProperty("tagType");
        }
        else
        {
            _serializedObj = null;
            _labelProp = null;
            _iconProp = null;
            _typeProp = null;
        }

        rootVisualElement.Clear();
        CreateGUI();
    }

    [MenuItem("Assets/Open in TagView", true)]
    private static bool ValidateOpen()
    {
        return Selection.activeObject is CustomTag;
    }

    [MenuItem("Assets/Open in TagView")]
    private static void OpenSelected()
    {
        if (Selection.activeObject is CustomTag data)
        {
            Open(data);
        }
    }
    [MenuItem("Window/UI Toolkit/TagView")]
    public static void ShowExample()
    {
        TagView wnd = GetWindow<TagView>();
        wnd.titleContent = new GUIContent("TagView");
    }

    public void CreateGUI()
    {
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
        _label.value = "Tag Name";
        if (_labelProp != null) _label.BindProperty(_labelProp);
        infoView.Add(_label);

        //Category and Icon View
        var splitViewD = new TwoPaneSplitView(0, 315, TwoPaneSplitViewOrientation.Horizontal);
        splitViewD.style.maxHeight = 128;
        infoView.Add(splitViewD);

        var descLeft = new VisualElement();
        splitViewD.Add(descLeft);
        var descRight = new VisualElement();
        splitViewD.Add(descRight);

        _tagType = new EnumField("Tag Type", TagType.Core);
        _tagType.style.paddingRight = 4;
        _tagType.style.paddingTop = 4;
        if (_typeProp != null) _tagType.BindProperty(_typeProp);
        descLeft.Add(_tagType);

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

        //Buttons
        var splitViewB = new TwoPaneSplitView(0, 315, TwoPaneSplitViewOrientation.Horizontal);
        splitViewB.style.maxHeight = 36;
        root.Add(splitViewB);

        var save = new Button(() => SaveInfo());
        save.text = "Save";
        splitViewB.Add(save);

        var close = new Button(() => this.Close());
        close.text = "Close";
        splitViewB.Add(close);
    }

    private void UpdateIconPreview(Sprite sprite)
    {
        _preview.image = sprite != null ? sprite.texture : null;
    }
    private void SaveInfo()
    {
        var formatID = _label.value;
        formatID = formatID.Replace(" ", "_");
        formatID = formatID.ToLower();
        if (_obj == null || _serializedObj == null)
        {

            var data = ScriptableObject.CreateInstance<CustomTag>();
            data.id = "tag_" + formatID;
            data.label = _label.value;
            data.icon = _icon.value as Sprite;
            data.category = (TagType)_tagType.value;

            AssetDatabase.CreateAsset(data, "Assets/Data/Tags/" + _label.value + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SetData(data);
        }
        else
        {
            _serializedObj.Update();

            var id = _serializedObj.FindProperty("id");
            id.stringValue = "tag_" + formatID;

            _serializedObj.ApplyModifiedProperties();
            EditorUtility.SetDirty(_obj);
            AssetDatabase.SaveAssets();
        }
    }
}
