using Codice.Client.BaseCommands;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class AssetView : EditorWindow
{
    private List<CustomTag> tags = new();
    private List<AbilityData> abilities = new();
    private List<MonsterData> monsters = new();
    private List<ItemData> items = new();
    private int currentTab = 0;

    private List<(Button element, CustomTag data)> tTiles = new();
    private List<(Button element, AbilityData data)> aTiles = new();
    private List<(Button element, MonsterData data)> mTiles = new();
    private List<(Button element, ItemData data)> iTiles = new();

    private Dictionary<string, VisualElement> tabFilterContainers = new();
    private Dictionary<string, HashSet<CustomTag>> activeFilters = new();
    private Dictionary<string, List<(Button element, CustomTag data)>> filterBtns = new();
    private string searchText = "";

    [MenuItem("Window/UI Toolkit/AssetView")]
    public static void ShowExample()
    {
        AssetView wnd = GetWindow<AssetView>();
        wnd.titleContent = new GUIContent("AssetView");
    }
    private void OnFocus()
    {
        rootVisualElement.Clear();
        CreateGUI();
    }
    public void CreateGUI()
    {
        GatherData();

        rootVisualElement.Clear();
        VisualElement root = rootVisualElement;
        root.style.flexDirection = FlexDirection.Column;
        root.style.flexGrow = 1;

        var tabs = new TabView();
        tabs.style.flexGrow = 1;
        tabs.schedule.Execute(() =>
        {
            if(currentTab != tabs.selectedTabIndex)
            {
                currentTab = tabs.selectedTabIndex;
            }
            ApplyAllFilters();
            UpdateFilterVisuals();
        }).Every(100);

        root.Add(tabs);

        //Tags Tab
        var tagTab = new Tab("Tags");
        tabs.Add(tagTab);

        SetupTab(tagTab,
            tags,
            tTiles,
            a => TagView.Open(a),
            a => a.icon,
            a => a.label,
            () => TagView.Open(null));

        //Ability Tab
        var abiTab = new Tab("Abilities");
        tabs.Add(abiTab);

        SetupTab(abiTab,
            abilities,
            aTiles,
            a => AbilityView.Open(a),
            a => a.icon,
            a => a.label,
            () => AbilityView.Open(null)
        );

        //Monster Tab
        var monTab = new Tab("Monsters");
        tabs.Add(monTab);

        SetupTab(monTab,
            monsters,
            mTiles,
            a => MonsterView.Open(a),
            a => a.icon,
            a => a.label,
            () => MonsterView.Open(null)
        );

        //Item Tab
        var itemTab = new Tab("Items");
        tabs.Add(itemTab);

        SetupTab(itemTab,
            items,
            iTiles,
            a => ItemView.Open(a),
            a => a.icon,
            a => a.label,
            () => ItemView.Open(null)
        );

        tabs.selectedTabIndex = currentTab;
        RestoreFilterState();
    }

    private void ApplyAllFilters()
    {
        ApplyFilters(tTiles);
        ApplyFilters(aTiles);
        ApplyFilters(mTiles);
        ApplyFilters(iTiles);
    }
    private void GatherData()
    {
        string[] guidsT = AssetDatabase.FindAssets("t:CustomTag", new[] { "Assets/Data/Tags" });
        string[] guidsA = AssetDatabase.FindAssets("t:AbilityData", new[] { "Assets/Data/Abilities" });
        string[] guidsM = AssetDatabase.FindAssets("t:MonsterData", new[] { "Assets/Data/Monsters" });
        string[] guidsI = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Data/Items" });

        tags = new List<CustomTag>();
        abilities = new List<AbilityData>();
        monsters = new List<MonsterData>();
        items = new List<ItemData>();

        foreach(string guid in guidsT)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CustomTag tag = AssetDatabase.LoadAssetAtPath<CustomTag>(path);

            if(tag != null)
            {
                tags.Add(tag);
            }
        }
        foreach (string guid in guidsA)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AbilityData ability = AssetDatabase.LoadAssetAtPath<AbilityData>(path);

            if (ability != null)
            {
                abilities.Add(ability);
            }
        }
        foreach (string guid in guidsM)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonsterData monster = AssetDatabase.LoadAssetAtPath<MonsterData>(path);

            if (monster != null)
            {
                monsters.Add(monster);
            }
        }
        foreach (string guid in guidsI)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);

            if (item != null)
                items.Add(item);
        }
    }
    private void SetupTab<T>(Tab tab, List<T> data, List<(Button element, T data)> tileList, Action<T> onClick, Func<T, Sprite> getIcon, Func<T, string> getLabel, Action onCreate) where T : EntityData
    {
        tileList.Clear();
        if (!activeFilters.ContainsKey(tab.label))
        {
            activeFilters.Add(tab.label, new HashSet<CustomTag>());
            filterBtns.Add(tab.label, new List<(Button element, CustomTag data)>());
        }
        var popupSelections = new Dictionary<FieldInfo, string>();

        tab.contentContainer.style.flexDirection = FlexDirection.Column;
        tab.contentContainer.style.flexGrow = 1;

        //Search Section
        var searchContainer = new VisualElement();
        searchContainer.style.flexDirection = FlexDirection.Row;
        tab.Add(searchContainer);

        var search = new ToolbarSearchField();
        search.style.flexGrow = 1;
        search.RegisterValueChangedCallback(evt =>
        {
            searchText = evt.newValue;
            ApplyFilters(tileList);
        });
        searchContainer.Add(search);

        //Filterable Options
        var optionContainer = new VisualElement();
        optionContainer.style.flexDirection = FlexDirection.Row;
        optionContainer.style.flexWrap = Wrap.Wrap;
        optionContainer.style.display = DisplayStyle.None;
        if (!tabFilterContainers.ContainsKey(tab.label))
            tabFilterContainers.Add(tab.label, optionContainer);
        else
            tabFilterContainers[tab.label] = optionContainer;

            var filter = new Button(() =>
            {
                optionContainer.style.display = optionContainer.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
            });
        filter.text = "Filter Options";
        searchContainer.Add(filter);
        tab.Add(optionContainer);
        
        foreach(var tag in tags)
        {
            var filterTile = CreateFilterTile(tag.icon,
                tag.label,
                () => ToggleFilter(tileList, tag));
            optionContainer.Add(filterTile);
            filterBtns[tab.label].Add((filterTile, tag));
        }
        if (tab.label == "Tags")
            filter.style.display = DisplayStyle.None;

        //Main Area
        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Column;
        container.style.flexGrow = 1;
        container.style.minHeight = 0;
        tab.Add(container);

        var scroll = new ScrollView();
        scroll.style.flexGrow = 1;
        container.Add(scroll);

        var list = new VisualElement();
        list.style.flexDirection = FlexDirection.Row;
        list.style.flexWrap = Wrap.Wrap;
        scroll.Add(list);

        foreach(var item in data)
        {
            var tile = CreateTile(
                getIcon(item),
                getLabel(item),
                () => onClick(item)
            );
            tileList.Add((tile, item));

            list.Add(tile);
        }

        //Buttons
        var add = new Button(onCreate);
        add.text = $"Create {typeof(T).Name.Replace("Data", "")}";
        add.style.flexShrink = 0;
        add.style.marginTop = 4;
        container.Add(add);
    }
    private void ToggleFilter<T>(List<(Button element, T data)> list, CustomTag tag) where T : EntityData
    {
        if (tag == null) return;

        switch(currentTab)
        {
            case 0:
                if (activeFilters["Tags"].Contains(tag))
                    activeFilters["Tags"].Remove(tag);
                else
                    activeFilters["Tags"].Add(tag);
                break;
            case 1:
                if (activeFilters["Abilities"].Contains(tag))
                    activeFilters["Abilities"].Remove(tag);
                else
                    activeFilters["Abilities"].Add(tag);
                break;
            case 2:
                if (activeFilters["Monsters"].Contains(tag))
                    activeFilters["Monsters"].Remove(tag);
                else
                    activeFilters["Monsters"].Add(tag);
                break;
            case 3:
                if (activeFilters["Items"].Contains(tag))
                    activeFilters["Items"].Remove(tag);
                else
                    activeFilters["Items"].Add(tag);
                break;
        }

        UpdateFilterVisuals();
        ApplyFilters(list);
    }
    private bool MatchesSearch<T>(T data) where T : EntityData
    {
        if (string.IsNullOrEmpty(searchText)) return true;

        return data.label.ToLower().Contains(searchText.ToLower());
    }
    private bool MatchesTags<T>(T data) where T : EntityData
    {
        switch(currentTab)
        {
            case 0:
                if (activeFilters["Tags"].Count == 0) return true;

                if (data.tagList == null) return false;

                foreach (var tag in activeFilters["Tags"])
                {
                    if (!data.tagList.Contains(tag)) return false;
                }
                break;
            case 1:
                if (activeFilters["Abilities"].Count == 0) return true;

                if (data.tagList == null) return false;

                foreach (var tag in activeFilters["Abilities"])
                {
                    if (!data.tagList.Contains(tag)) return false;
                }
                break;
            case 2:
                if (activeFilters["Monsters"].Count == 0) return true;

                if (data.tagList == null) return false;

                foreach (var tag in activeFilters["Monsters"])
                {
                    if (!data.tagList.Contains(tag)) return false;
                }
                break;
            case 3:
                if (activeFilters["Items"].Count == 0) return true;

                if (data.tagList == null) return false;

                foreach (var tag in activeFilters["Items"])
                {
                    if (!data.tagList.Contains(tag)) return false;
                }
                break;
        }

        return true;
    }
    private void ApplyFilters<T>(List<(Button element, T data)> list) where T : EntityData
    {
        var search = string.IsNullOrEmpty(searchText) ? "" :
            searchText.ToLower();

        foreach((Button element, T data) in list)
        {
            bool match = (MatchesSearch(data) && MatchesTags(data));

            if (!match)
                element.style.display = DisplayStyle.None;
            else
                element.style.display = DisplayStyle.Flex;
        }
    }
    private void RestoreFilterState()
    {
        foreach(var kvp in activeFilters)
        {
            var filters = kvp.Value;
            tabFilterContainers[kvp.Key].style.display = filters.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    private void UpdateFilterVisuals()
    {
        foreach (var kvp in filterBtns)
        {
            var filters = activeFilters[kvp.Key];
            var buttons = kvp.Value;
            foreach(var (b, d) in buttons)
            {
                if (filters.Contains(d))
                    b.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                else
                    b.style.backgroundColor = new StyleColor(StyleKeyword.Null);
            }
        }
    }
    private Button CreateFilterTile(Sprite sprite, string labelText, Action onClick)
    {
        var button = new Button(() => onClick?.Invoke());
        button.style.minWidth = 44;
        button.style.height = 44;
        button.style.marginRight = 6;
        button.style.marginBottom = 6;
        button.style.alignItems = Align.Center;
        button.style.justifyContent = Justify.Center;
        button.style.flexDirection = FlexDirection.Column;

        var icon = new Image();
        if (sprite != null)
            icon.image = sprite.texture;
        icon.scaleMode = ScaleMode.ScaleToFit;
        icon.style.width = 16;
        icon.style.height = 16;
        icon.style.marginBottom = 4;

        var label = new Label(labelText);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.fontSize = 10;

        button.Add(icon);
        button.Add(label);

        return button;
    }
    private Button CreateTile(Sprite sprite, string labelText, Action onClick)
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
        if (sprite != null)
            icon.image = sprite.texture;
        icon.scaleMode = ScaleMode.ScaleToFit;
        icon.style.width = 40;
        icon.style.height = 40;
        icon.style.marginBottom = 4;

        var label = new Label(labelText);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.fontSize = 10;

        button.Add(icon);
        button.Add(label);

        return button;
    }
}
