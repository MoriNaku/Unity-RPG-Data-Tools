using PlasticGui.Help.Actions;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro.EditorUtilities;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class LootEntryView : EditorWindow
{
    private List<ItemData> items = new();
    private ItemData selectedItem;
    private List<LootEntry> currentLoot = new();

    private LootTableData _obj;
    private SerializedObject _serializedObj;
    private SerializedProperty _entriesProp;

    private PopupField<ItemData> _item;
    private IntegerField _amount;
    private FloatField _chance;
    private ScrollView _lootList;

    [MenuItem("Assets/Open in LootEntryView", true)]
    private static bool ValidateOpen()
    {
        return Selection.activeObject is LootTableData;
    }

    [MenuItem("Assets/Open in LootEntryView")]
    private static void OpenSelected()
    {
        if (Selection.activeObject is LootTableData data)
        {
            Open(data);
        }
    }
    [MenuItem("Window/UI Toolkit/LootEntryView")]
    public static void ShowExample()
    {
        LootEntryView wnd = GetWindow<LootEntryView>();
        wnd.titleContent = new GUIContent("LootEntryView");
    }
    private void OnFocus()
    {
        rootVisualElement.Clear();
        CreateGUI();
    }
    public static void Open(LootTableData data)
    {
        var wnd = GetWindow<LootEntryView>();
        wnd.titleContent = new GUIContent("LootEntryView");
        wnd.GetItems();
        wnd.SetData(data);
    }
    private void SetData(LootTableData data)
    {
        _obj = data;

        if (_obj != null)
        {
            currentLoot = _obj.table ?? new List<LootEntry>();
            _serializedObj = new SerializedObject(_obj);
            _entriesProp = _serializedObj.FindProperty("table");
        } else
        {
            currentLoot = new List<LootEntry>();
            _serializedObj = null;
            _entriesProp = null;
        }

        rootVisualElement.Clear();
        CreateGUI();
    }

    private void GetItems()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemData");

        items = new List<ItemData>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);

            if (item != null)
            {
                items.Add(item);
            }
        }
    }
    public void CreateGUI()
    {
        rootVisualElement.Clear();
        VisualElement root = rootVisualElement;
        root.style.flexDirection = FlexDirection.Column;

        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Column;
        root.Add(container);

        var titleC = new VisualElement();
        titleC.style.flexDirection = FlexDirection.Row;
        titleC.style.justifyContent = Justify.Center;
        titleC.style.alignItems = Align.Center;
        container.Add(titleC);

        var title = new Label();
        title.text = _obj == null ? "Loot Table" : _obj.label;
        title.style.fontSize = 24;
        title.style.paddingTop = 4;
        title.style.paddingRight = 4;
        title.style.paddingBottom = 4;
        title.style.paddingLeft = 4;
        title.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleC.Add(title);

        var draftView = new VisualElement();
        draftView.style.flexDirection = FlexDirection.Row;
        draftView.style.justifyContent = Justify.Center;
        draftView.style.alignContent = Align.Center;
        container.Add(draftView);

        _item = new PopupField<ItemData>(
            "Item",
            items,
            0,
            item => item != null ? item.label : "None",
            item => item != null ? item.label : "None"
        );
        _item.RegisterValueChangedCallback(evt =>
        {
            selectedItem = evt.newValue;
        });

        if (selectedItem != null && items.Contains(selectedItem))
        {
            _item.SetValueWithoutNotify(selectedItem);
        }

        draftView.Add(_item);

        _amount = new IntegerField("Amount");
        draftView.Add(_amount);

        var chanceC = new VisualElement();
        chanceC.style.flexDirection = FlexDirection.Row;
        draftView.Add(chanceC);

        _chance = new FloatField("Drop Chance");
        _chance.style.flexGrow = 1;
        chanceC.Add(_chance);

        var chanceL = new Label(" / 1.00");
        chanceC.Add(chanceL);
        draftView.Add(chanceC);

        var add = new Button(() => AddLoot());
        var add_icon = EditorGUIUtility.IconContent("Toolbar Plus").image;
        add.style.backgroundImage = add_icon as Texture2D;

        add.tooltip = "Add Entry";
        add.style.height = 24;
        add.style.width = 24;
        draftView.Add(add);

        //Entries
        _lootList = new ScrollView();
        _lootList.style.flexGrow = 1;
        container.Add(_lootList);
        RefreshLootList();

        //Buttons
        var splitViewB = new TwoPaneSplitView(0, 315, TwoPaneSplitViewOrientation.Horizontal);
        splitViewB.style.maxHeight = 36;
        container.Add(splitViewB);

        var save = new Button(() => SaveInfo());
        save.style.minHeight = 36;
        save.text = "Save";
        splitViewB.Add(save);

        var close = new Button(() => this.Close());
        close.style.minHeight = 36;
        close.text = "Close";
        splitViewB.Add(close);
    }
    private void AddLoot()
    {
        if (_obj == null || _serializedObj == null) return;
        if (_item.value == null) return;

        _serializedObj.Update();

        var tableProp = _serializedObj.FindProperty("table");
        if (tableProp == null || !tableProp.isArray) return;

        for (int i = 0; i < tableProp.arraySize; i++)
        {
            var element = tableProp.GetArrayElementAtIndex(i);
            var itemRef = element.FindPropertyRelative("item");

            if (itemRef != null && itemRef.objectReferenceValue == _item.value)
            {
                return;
            }
        }
        var index = tableProp.arraySize;
        tableProp.arraySize++;

        var newElement = tableProp.GetArrayElementAtIndex(index);

        var itemProp = newElement.FindPropertyRelative("item");
        var amountProp = newElement.FindPropertyRelative("amount");
        var chanceProp = newElement.FindPropertyRelative("chance");

        itemProp.objectReferenceValue = _item.value;
        amountProp.intValue = _amount.value;
        chanceProp.floatValue = _chance.value;

        _serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(_obj);
        AssetDatabase.SaveAssets();

        RefreshLootList();
    }
    private void RefreshLootList()
    {
        _lootList.Clear();

        for (int i = 0; i < currentLoot.Count; i++)
        {
            var item = currentLoot[i];
            var index = i;
            _lootList.Add(item.GetVisuals(() => ItemView.Open(item.item), () => _obj.RemoveEntryAt(index), true));
        }
    }
    private void SaveInfo()
    {
        if (_obj == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:LootTableData");
            var label = $"lootTable_{guids.Length}";
            var data = ScriptableObject.CreateInstance<LootTableData>();
            data.id = label;
            data.label = "";
            data.table = new();

            AssetDatabase.CreateAsset(data, "Assets/Data/Loot Tables/" + label + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SetData(data);
        }
        else
        {
            EditorUtility.SetDirty(_obj);
            AssetDatabase.SaveAssets();
        }
    }
}
