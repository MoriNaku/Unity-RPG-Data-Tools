using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "LootTableData", menuName = "Custom/Loot Table")]
public class LootTableData : EntityData
{
    public List<LootEntry> table = new();
    public override VisualElement GetVisuals(Action onClick = null, Action onDel = null, bool delete = false)
    {
        var container = new Button(() => onClick?.Invoke());
        container.style.flexDirection = FlexDirection.Column;

        var titleC = new VisualElement();
        titleC.style.flexDirection = FlexDirection.Row;
        titleC.style.justifyContent = Justify.Center;
        titleC.style.alignItems = Align.Center;
        container.Add(titleC);

        var title = new Label();
        title.text = "Loot Table";
        title.style.fontSize = 24;
        title.style.paddingTop = 4;
        title.style.paddingRight = 4;
        title.style.paddingBottom = 4;
        title.style.paddingLeft = 4;
        title.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleC.Add(title);
        return container;
    }

    public void RemoveEntryAt(int index)
    {
        table.RemoveAt(index);
    }
}

[Serializable]
public class LootEntry
{
    public ItemData item;
    public int amount;
    public float chance;

    public VisualElement GetVisuals(Action onClick, Action onDel = null, bool delete = false)
    {
        var container = new Button(() => onClick?.Invoke());
        container.style.flexDirection = FlexDirection.Row;

        var itemC = new VisualElement();
        itemC.style.flexDirection = FlexDirection.Row;
        itemC.style.flexGrow = 1;
        container.Add(itemC);

        var icon = new Image();
        icon.style.height = 24;
        icon.style.width = 24;
        icon.scaleMode = ScaleMode.ScaleToFit;
        if (item.icon != null) icon.image = item.icon.texture;
        itemC.Add(icon);

        var label = new Label();
        label.text = item.label;
        label.style.flexGrow = 1;
        itemC.Add(label);

        var chanceL = new Label();
        chanceL.text = $"{amount}x {chance:P00}%";
        chanceL.style.flexGrow = 1;
        container.Add(chanceL);

        var del = new Button(() => onDel?.Invoke());
        var del_icon = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
        del.style.backgroundImage = del_icon as Texture2D;

        del.tooltip = "Delete";
        del.style.height = 24;
        del.style.width = 24;
        container.Add(del);
        if (!delete)
            del.style.display = DisplayStyle.None;

        return container;
    }
}