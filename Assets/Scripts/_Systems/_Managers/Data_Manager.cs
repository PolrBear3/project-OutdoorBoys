using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data_Manager : MonoBehaviour
{
    public static Data_Manager instance;

    [Space(20)]
    [SerializeField] private TileScrObj[] _tileScrObjs;
    public TileScrObj[] tileScrObjs => _tileScrObjs;

    [Space(20)]
    [SerializeField] private Item_ScrObj[] _itemScrObjs;
    public Item_ScrObj[] itemScrObjs => _itemScrObjs;

    /*
    [SerializeField] private UseableItem[] _useableItems;
    public UseableItem[] useableItems => _useableItems;
    */


    // MonoBehaviour
    private void Awake()
    {
        instance = this;
    }


    // tileScrObjs
    public List<TileScrObj> TileScrObjs(TileType tileType)
    {
        List<TileScrObj> tiles = new();

        for (int i = 0; i < _tileScrObjs.Length; i++)
        {
            if (_tileScrObjs[i].type != tileType) continue;
            tiles.Add(_tileScrObjs[i]);
        }

        return tiles;
    }

    public TileScrObj TileScrObj(TileType tileType)
    {
        List<TileScrObj> tiles = TileScrObjs(tileType);
        int randIndex = Random.Range(0, tiles.Count);

        if (tiles.Count <= 0) return null;
        return tiles[randIndex];
    }


    // _itemScrObjs
    public Item_ScrObj ItemScrObj(string itemName)
    {
        for (int i = 0; i < _itemScrObjs.Length; i++)
        {
            if (itemName != _itemScrObjs[i].itemName) continue;
            return _itemScrObjs[i];
        }
        return null;
    }
}