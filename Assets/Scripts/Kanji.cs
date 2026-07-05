using System.Collections.Generic;
using UnityEngine;


enum Type
{
    none,
    core,
    target,
    mod
}
public class Kanji
{
    public string Name, Desc;
    List<string> BaseTags, TargetTags, ModTags;
    Type KanjiType;

    public Kanji(string _name)
    {
        Name = _name;
        Desc = "";
        KanjiType = Type.none;
    }

    public Kanji(string _name, string _desc)
    {
        Name = _name;
        Desc = _desc;
        KanjiType = Type.none;
    }

    public void AddTag(int _locator, string _tag)
    {
        switch (_locator)
        {
            case 0:
                BaseTags.Add(_tag);
                break;
            case 1:
                TargetTags.Add(_tag);
                break;
            case 2:
                ModTags.Add(_tag);
                break;
            default:
                Debug.Log("Incorrect Locator Given");
                break;
        }
    }

    public void SetTags(int _locator, string[] _tags)
    {
        switch(_locator)
        {
            case 0:
                BaseTags.Clear();
                BaseTags.AddRange(_tags);
                break;
            case 1:
                TargetTags.Clear();
                TargetTags.AddRange(_tags);
                break;
            case 2:
                ModTags.Clear();
                ModTags.AddRange(_tags);
                break;
            default:
                Debug.Log("Incorrect Locator Given");
                break;
        }
    }

    public void SetType(int _pointer)
    {
        switch(_pointer)
        {
            case 1:
                KanjiType = Type.core;
                break;
            case 2:
                KanjiType = Type.target;
                break;
            case 3:
                KanjiType = Type.mod;
                break;
            default:
                KanjiType = Type.none;
                break;
        }
    }

    public List<string> GetTags()
    {
        switch (KanjiType)
        {
            case Type.core:
                return BaseTags;
            case Type.target:
                return TargetTags;
            case Type.mod:
                return ModTags;
            default:
                return null;
        }
    }
}
