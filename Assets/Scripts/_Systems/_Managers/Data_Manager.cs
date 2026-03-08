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
    [SerializeField] private Item_ScrObj[] _allItems;
    public Item_ScrObj[] allItems => _allItems;

    [Space(20)]
    [SerializeField] private AnimalScrObj[] _allAnimals;
    public AnimalScrObj[] allAnimals => _allAnimals;


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
    public Item_ScrObj Item(string itemName)
    {
        for (int i = 0; i < _allItems.Length; i++)
        {
            if (itemName != _allItems[i].itemName) continue;
            return _allItems[i];
        }
        return null;
    }
}