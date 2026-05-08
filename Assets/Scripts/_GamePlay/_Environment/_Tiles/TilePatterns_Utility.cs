using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TilePatterns_Utility
{
    public static List<Tile> CheckBoard_Tiles(bool skipFirstTile)
    {
        InGame_Manager manager = InGame_Manager.instance;

        Tiles_Controller tilesController = manager.tilesController;
        List<Tile> allTiles = tilesController.currentTiles;

        if (allTiles.Count <= 0) return null;

        WorldMap_Generator generator = manager.worldMapGenerator;
        List<Tile> matchedTiles = new();

        int columnCount = Mathf.RoundToInt(generator.Converted_GenerateSize().x);

        for (int i = 0; i < allTiles.Count; i++)
        {
            int column = i % columnCount;
            int row = i / columnCount;

            bool isEven = (row + column) % 2 == 0;
            if (isEven == skipFirstTile) continue;

            matchedTiles.Add(allTiles[i]);
        }
        return matchedTiles;
    }

    public static List<Tile> PivotDistanced_Tiles(Tile pivotTile, int distanceRange)
    {
        if (pivotTile == null) return null;

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;
        List<Tile> allTiles = tilesController.currentTiles;

        List<Tile> innerRangedTiles = new();

        for (int i = 0; i < allTiles.Count; i++)
        {
            Tile tile = allTiles[i];
            float distance = Utility.Chebyshev_Distance(pivotTile.transform.position, tile.transform.position);

            if (distance > distanceRange) continue;
            innerRangedTiles.Add(tile);
        }
        return innerRangedTiles;
    }

    public static List<Tile> StraightRow_Tiles(Tile pivotTile, bool isIncrement)
    {
        return null;
    }

    public static List<Tile> StraightColumn_Tiles(Tile pivotTile, bool isIncrement)
    {
        return null;
    }
}
