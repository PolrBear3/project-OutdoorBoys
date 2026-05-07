using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TilePattern : MonoBehaviour
{
    public abstract List<Tile> PatternMatch_Tiles(List<Tile> allTiles);
}