using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Item Rule/ Exclude Tiles")]
public class ItemRule_ExcludeTiles : ItemRule_ScrObj
{
    [Space(20)]
    [SerializeField] private TileScrObj[] _excludeTiles;
    
    public override bool Available(ItemData _, Tile targetTile)
    {
        for (int i = 0; i < _excludeTiles.Length; i++)
        {
            if (targetTile.data.tileScrObj == _excludeTiles[i]) return false;
        }
        return true;
    }
}