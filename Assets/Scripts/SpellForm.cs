using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SpellForm
{
    public string Name;
    private Kanji[] Glyphs;
    private List<string> GlyphTags;

    public SpellForm()
    {
        Name = "";
        Glyphs = new Kanji[3];
        GlyphTags = new List<string>();
    }

    public void SetGlyph(Kanji _kanji, int _type, int _pointer)
    {
        Kanji temp = _kanji;
        temp.SetType(_type);
        List<string> _tags = temp.GetTags();
        if(_tags != null)
        {
            foreach(string t in _tags)
            {
                if(!GlyphTags.Contains(t)) GlyphTags.Add(t);
            }
        }
        Glyphs[_pointer] = _kanji;
        UpdateName();
    }

    public void RemoveGlyph(int _pointer)
    {
        Glyphs[_pointer] = null;
        UpdateName();
    }

    private void UpdateName()
    {
        Name = "";
        if (Glyphs[0] != null) Name += Glyphs[0].Name;
        if (Glyphs[1] != null) Name += Glyphs[1].Name;
        if (Glyphs[2] != null) Name += Glyphs[2].Name;
    }

    public string[] GetTags()
    {
        return GlyphTags.ToArray();
    }
}
