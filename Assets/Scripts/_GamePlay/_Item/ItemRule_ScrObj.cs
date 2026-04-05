using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Item Rule")]
public abstract class ItemRule_ScrObj : ScriptableObject
{
    public virtual bool Available(ItemData currentData, Tile targetTile)
    {
        return true;
    }
}