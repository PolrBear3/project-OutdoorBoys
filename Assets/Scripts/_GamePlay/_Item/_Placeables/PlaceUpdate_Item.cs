using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceUpdate_Item : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField] private TileUpdate_ItemData[] _placeUpdateDatas;
    public TileUpdate_ItemData[] placeUpdateDatas => _placeUpdateDatas;


    // MonoBehaviour
    private void Start()
    {
        InGame_Manager.instance.time.Register(ActionUpdateBus.AwakeUpdate, PlaceUpdate);
    }
    
    private void OnDestroy()
    {
        InGame_Manager.instance.time.UnRegister(ActionUpdateBus.AwakeUpdate, PlaceUpdate);
    }
    
 
    // Main
    private void PlaceUpdate()
    {
        Tile currentTile = _placeableItem.placedTile;
        List<Tile> surroundingTiles = TilePatterns_Utility.PivotDistanced_Tiles(currentTile, 1);

        for (int i = 0; i < _placeUpdateDatas.Length; i++)
        {
            TileUpdate_ItemData data = _placeUpdateDatas[i];

            if (data.UpdateTile_Match(currentTile) == false) continue;
            if (data.AllTiles_ItemsPlaced(currentTile, surroundingTiles) == false) continue;
            
            _placeableItem.AnimationDelay_Remove();

            ItemData updateData = data.Update_ItemData();
            if (updateData == null) return;

            currentTile.SetPreserve_Item(updateData);
            return;
        }
    }
}
