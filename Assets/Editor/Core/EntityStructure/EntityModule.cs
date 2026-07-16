using System;
using UnityEngine;

[Serializable]
public abstract class EntityModule
{
    public virtual string RemoveWarning =>
        "Any data stored in this module will be lost.";
}
