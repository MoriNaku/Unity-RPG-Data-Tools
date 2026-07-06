using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public enum TagType
{
    Core
}

[CreateAssetMenu(menuName = "Custom/Tag")]
public class CustomTag : EntityData
{
    public Sprite icon;
    public TagType category;

    public override VisualElement GetVisuals(Action action, Action del_action, bool delete = false)
    {
        var container = new Button(() => action?.Invoke());
        container.style.flexDirection = FlexDirection.Row;
        container.style.marginTop = 4;
        container.style.paddingTop = 4;
        container.style.paddingRight = 4;
        container.style.paddingBottom = 4;
        container.style.paddingLeft = 4;

        var img = new Image();
        if(icon != null) img.image = icon.texture;
        img.style.height = 32;
        img.style.width = 32;
        container.Add(img);

        var text = new Label();
        text.text = label;
        text.style.unityTextAlign = TextAnchor.MiddleCenter;
        text.style.flexGrow = 1;
        text.style.paddingLeft = 4;
        container.Add(text);

        var del = new Button(() => del_action?.Invoke());
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
