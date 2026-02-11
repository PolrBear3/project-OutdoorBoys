using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile_PresetDatas
{
    [SerializeField] private TileScrObj _tileScrObj;
    public TileScrObj tileScrObj => _tileScrObj;

    [SerializeField][Range(0, 100)] private int _generateAmount;
    public int generateAmount => _generateAmount; 
}
