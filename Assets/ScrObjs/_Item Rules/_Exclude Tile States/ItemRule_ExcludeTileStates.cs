using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Item Rule/ Exclude Tile States")]
public class ItemRule_ExcludeTileStates : ItemRule_ScrObj
{
    [Space(20)]
    [SerializeField] private TileState[] _excludeStates;

    public override bool Available(ItemData _, Tile targetTile)
    {
        for (int i = 0; i < _excludeStates.Length; i++)
        {
            List<TileState_Data> tileStateDatas = new(targetTile.data.stateDatas);

            for (int j = 0; j < tileStateDatas.Count; j++)
            {
                if (_excludeStates[i] == tileStateDatas[j].tileState) return false;
            }
        }
        return true;
    }
}
