using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class TagModule : EntityModule
{
    public List<CustomTag> auto = new();
    public List<CustomTag> manual = new();
}
