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
    private int currentTab = 0;

    private string[] myModules = {
        "AbilityData",
        "ActorData",
        "ItemData"
    };

    private List<(Button element, ScriptableObject data)> tTiles = new();

    private Dictionary<string, VisualElement> tabFilterContainers = new();
    private Dictionary<string, HashSet<CustomTag>> activeFilters = new();
    private Dictionary<string, List<(Button element, CustomTag data)>> filterBtns = new();
    private string searchText = "";

    public CoreDataRegistry registry = null;
    private Dictionary<int, List<ScriptableObject>> masterList = new();

    public Type GetScriptableObjectType(MonoScript script)
    {
        if (script == null) return null;

        Type type = script.GetClass();

        if(type == null) return null;

        if (!typeof(ScriptableObject).IsAssignableFrom(type))
        {
            Debug.LogWarning($"{type.Name} is not a ScriptableObject!");
            return null;
        }

        return type;
    }

    public static void RefreshOpenWindows()
    {
        foreach(var window in Resources.FindObjectsOfTypeAll<AssetView>())
        {
            window.CreateGUI();
        }
    }

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
    private string GetCurrentTabName(TabView tabs)
    {
        if (tabs == null) return "";

        int index = tabs.selectedTabIndex;

        if (index < 0 || index >= tabs.childCount)
            return "";

        if (tabs[index] is Tab selectedTab)
            return selectedTab.label;

        return "";
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

            string currentTabName = GetCurrentTabName(tabs);

            ApplyAllFilters(currentTabName);
            UpdateFilterVisuals();
        }).Every(100);

        root.Add(tabs);

        //Core Tabs
        var tagTab = new Tab("Tags");
        tabs.Add(tagTab);

        SetupCoreTab(tagTab,
            tags,
            tTiles,
            a => TagView.Open(a),
            a => a.icon,
            a => a.label,
            () => TagView.Open(null));

        //Generic
        for (int i = 0; i < registry.moduleList.Count; i++)
        {
            if (!masterList.ContainsKey(i)) continue;
            List<ScriptableObject> assets = masterList[i];
            var tab = new Tab(registry.moduleList[i].tabName);
            tabs.Add(tab);

            SetupTab(tab,
                registry.moduleList[i],
                assets,
                new List<(Button button, ScriptableObject data)>(),
                a => a.name);
        }

        /*
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

        //Actor Tab
        var monTab = new Tab("Actors");
        tabs.Add(monTab);

        SetupTab(monTab,
            actors,
            mTiles,
            a => ActorView.Open(a),
            a => a.icon,
            a => a.label,
            () => ActorView.Open(null)
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
        */

        tabs.selectedTabIndex = currentTab;
        RestoreFilterState();
    }

    private void ApplyAllFilters(string tabName)
    {
        ApplyFilters(tabName, tTiles);
    }
    private void GatherData()
    {
        masterList.Clear();

        if (registry == null)
        {
            string[] found = AssetDatabase.FindAssets("t:CoreDataRegistry");
            if(found.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(found[0]);
                registry = AssetDatabase.LoadAssetAtPath<CoreDataRegistry>(path);
            }

            if(registry == null)
            {
                Debug.LogWarning("No CoreDataRegistry found.");
                return;
            }
            if(registry.moduleList.Count > 0 || registry.moduleList == null)
            {
                Debug.LogWarning("Add Modules to Registry Before Proceeding!");
                return;
            }
        }

        for (int i = 0; i < registry.moduleList.Count; i++)
        {
            DataModule module = registry.moduleList[i];
            if (module == null) continue;
            if (module.moduleType == null) continue;
            if (string.IsNullOrEmpty(module.dataPath)) continue;

            Type type = GetScriptableObjectType(module.moduleType);
            if (type == null) continue;

            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}", new[] { module.dataPath });

            List<ScriptableObject> assets = new();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                ScriptableObject asset = AssetDatabase.LoadAssetAtPath(path, type) as ScriptableObject;

                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            masterList.Add(i, assets);
        }

        //Core Information
        string[] guidsT = AssetDatabase.FindAssets("t:CustomTag", new[] { "Assets/Data/Tags" });

        tags = new List<CustomTag>();
        foreach (string guid in guidsT)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CustomTag tag = AssetDatabase.LoadAssetAtPath<CustomTag>(path);

            if (tag != null)
            {
                tags.Add(tag);
            }
        }
        /*
        string[] guidsA = AssetDatabase.FindAssets("t:AbilityData", new[] { "Assets/Data/Abilities" });
        string[] guidsM = AssetDatabase.FindAssets("t:ActorData", new[] { "Assets/Data/Actors" });
        string[] guidsI = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Data/Items" });

        abilities = new List<AbilityData>();
        actors = new List<ActorData>();
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
            ActorData monster = AssetDatabase.LoadAssetAtPath<ActorData>(path);

            if (monster != null)
            {
                actors.Add(monster);
            }
        }
        foreach (string guid in guidsI)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);

            if (item != null)
                items.Add(item);
        }
        */
    }
    private void SetupTab(Tab tab, DataModule info, List<ScriptableObject> data, List<(Button button, ScriptableObject data)> tileList, Func<ScriptableObject, string> getLabel)
    {
        tileList.Clear();

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
            ApplyFilters(tab.label, tileList);
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
        if(info.usesTagFiltering)
        {
            searchContainer.Add(filter);
            tab.Add(optionContainer);
        }

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

        Type viewType = info.viewType.GetClass();
        MethodInfo openMethod = viewType.GetMethod(
            "Open",
            BindingFlags.Public | BindingFlags.Static);

        foreach (var item in data)
        {
            if (item is EntityData entity)
            {
                var tile = CreateTile(
                    entity.icon,
                    entity.label,
                    () => openMethod?.Invoke(null, new object[] { entity })
                );
                tileList.Add((tile, entity));

                list.Add(tile);
            } else
            {
                var tile = CreateTile(
                    null,
                    item.name,
                    null);
                tileList.Add((tile, item));

                list.Add(tile);
            }
        }

        //Buttons
        var add = new Button(() => openMethod?.Invoke(null, new object[] { null }));
        add.text = $"Create {info.displayName}";
        add.style.flexShrink = 0;
        add.style.marginTop = 4;
        if(info.usesCustomView)
            container.Add(add);
    }
    private void SetupCoreTab<T>(Tab tab, List<T> data, List<(Button element, ScriptableObject data)> tileList, Action<T> onClick, Func<T, Sprite> getIcon, Func<T, string> getLabel, Action onCreate) where T : EntityData
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
            ApplyFilters(tab.label, tileList);
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
                () => ToggleFilter(tab.label, tileList, tag));
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
    private void ToggleFilter(string tabName, List<(Button element, ScriptableObject data)> list, CustomTag tag)
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
                if (activeFilters["Actors"].Contains(tag))
                    activeFilters["Actors"].Remove(tag);
                else
                    activeFilters["Actors"].Add(tag);
                break;
            case 3:
                if (activeFilters["Items"].Contains(tag))
                    activeFilters["Items"].Remove(tag);
                else
                    activeFilters["Items"].Add(tag);
                break;
        }

        UpdateFilterVisuals();
        ApplyFilters(tabName, list);
    }
    private bool MatchesSearch<T>(T data) where T : ScriptableObject
    {
        if (string.IsNullOrEmpty(searchText)) return true;

        return data.name.ToLower().Contains(searchText.ToLower());
    }
    private bool MatchesTags(string tabName, ScriptableObject data)
    {
        if (!activeFilters.ContainsKey(tabName)) return true;

        if (activeFilters[tabName].Count == 0) return true;

        if (data is not EntityData entity) return false;

        if (entity.tagList == null) return false;

        foreach (var tag in activeFilters[tabName])
        {
            if (!entity.tagList.Contains(tag)) return false;
        }

        return true;
    }
    private void ApplyFilters(string tabName, List<(Button element, ScriptableObject data)> list)
    {
        var search = string.IsNullOrEmpty(searchText) ? "" :
            searchText.ToLower();

        foreach((Button element, ScriptableObject data) in list)
        {
            bool match = (MatchesSearch(data) && MatchesTags(tabName, data));
            
            element.style.display = match ? DisplayStyle.Flex : DisplayStyle.None;
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
