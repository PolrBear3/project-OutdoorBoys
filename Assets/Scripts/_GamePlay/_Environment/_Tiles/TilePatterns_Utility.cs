using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TilePatterns_Utility
{
    public static List<Tile> CheckBoard_Tiles(bool skipFirstTile)
    {
        InGame_Manager manager = InGame_Manager.instance;
        List<Tile> allTiles = manager.tilesController.currentTiles;

        if (allTiles.Count <= 0) return null;

        Vector2 startPos = manager.worldMapGenerator.Generate_StartPosition();
        List<Tile> matchedTiles = new();

        for (int i = 0; i < allTiles.Count; i++)
        {
            Tile tile = allTiles[i];
            Vector2 tilePos = tile.transform.position;

            int column = Mathf.RoundToInt(tilePos.x - startPos.x);
            int row = Mathf.RoundToInt(startPos.y - tilePos.y);
            
            bool isEven = (row + column) % 2 == 0;
            if (isEven == skipFirstTile) continue;
            
            matchedTiles.Add(tile);
        }

        return matchedTiles;
    }

    public static List<Tile> PivotDistanced_Tiles(Tile pivotTile, int distanceRange)
    {
        if (pivotTile == null) return null;

        List<Tile> allTiles = InGame_Manager.instance.tilesController.currentTiles;
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

    public static List<Tile> Directional_Tiles(Tile pivotTile, Vector2 direction)
    {
        if (pivotTile == null) return null;

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        List<Tile> allTiles = tilesController.currentTiles;
        List<Tile> directionalTiles = new();

        for (int i = 0; i < allTiles.Count; i++)
        {
            directionalTiles.Add(pivotTile);
            if (direction == Vector2.zero) break;

            pivotTile = tilesController.Current_Tile((Vector2)pivotTile.transform.position + direction);
            if (pivotTile == null) break;
        }
        return directionalTiles;

    }
}
