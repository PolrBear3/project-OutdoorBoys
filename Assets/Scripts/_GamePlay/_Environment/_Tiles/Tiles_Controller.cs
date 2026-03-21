using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles_Controller : MonoBehaviour, IItemsSource
{
    private List<Tile> _currentTiles = new();
    public List<Tile> currentTiles => _currentTiles;


    public Action<Tile> OnTileHover;

    public Action<Tile> OnTargetTileSelect;
    public Action<Tile> OnTargetTileHoldSelect;
    public Action<Tile> OnTileRightSelect;

    public Action OnTileSelect;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.Register(EventBus.StartLoad, Load_SetSprites);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Load_SetSprites);

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick -= Select_Tile;
        input.OnHoldLeftClick -= HoldSelect_Tile;
        input.OnRightClick -= RightSelect_Tile;

        InGame_Manager manager = InGame_Manager.instance;

        manager.cursor.OnTilePointRangeUpdate -= Refresh_Toggles;
        manager.player.movement.OnMovement -= Refresh_Toggles;
    }


    // IItemsSource
    public IEnumerable<ItemData> ItemDatas()
    {
        for (int i = 0; i < _currentTiles.Count; i++)
        {
            List<ItemData> placedItemDatas = _currentTiles[i].Placed_ItemDatas();

            for (int j = 0; j < placedItemDatas.Count; j++)
            {
                yield return placedItemDatas[j];
            }
        }
    }


    // Data
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick += Select_Tile;
        input.OnHoldLeftClick += HoldSelect_Tile;
        input.OnRightClick += RightSelect_Tile;

        InGame_Manager manager = InGame_Manager.instance;

        manager.cursor.OnTilePointRangeUpdate += Refresh_Toggles;
        manager.player.movement.OnMovement += Refresh_Toggles;
    }


    public List<Tile> Current_Tiles(TileScrObj sortingTile)
    {
        List<Tile> sortedTiles = new();

        for (int i = 0; i < _currentTiles.Count; i++)
        {
            Tile currentTile = _currentTiles[i];

            if (sortingTile != currentTile.data.tileScrObj) continue;
            sortedTiles.Add(currentTile);
        }

        if (sortedTiles.Count <= 0) return _currentTiles;
        return sortedTiles;
    }

    public List<Tile> Current_Tiles(Tile pivotTile, int rangeDistance)
    {
        if (pivotTile == null) return null;

        List<Tile> innerRangedTiles = new();

        for (int i = 0; i < _currentTiles.Count; i++)
        {
            Tile currentTile = _currentTiles[i];
            float distance = Utility.Chebyshev_Distance(pivotTile.transform.position, currentTile.transform.position);

            if (distance > rangeDistance) continue;
            innerRangedTiles.Add(currentTile);
        }
        return innerRangedTiles;
    }


    public Tile Current_Tile(Vector2 tileGeneratedPos)
    {
        for (int i = 0; i < _currentTiles.Count; i++)
        {
            if ((Vector2)_currentTiles[i].transform.position != tileGeneratedPos) continue;
            return _currentTiles[i];
        }
        return null;
    }

    /// <returns>
    /// random type matching tile, random tile among all current tiles if no matching tiles were found
    /// </returns>
    public Tile Current_Tile(TileType tileType)
    {
        List<Tile> matchTypeTiles = new();

        for (int i = 0; i < _currentTiles.Count; i++)
        {
            if (tileType != _currentTiles[i].data.tileScrObj.type) continue;
            matchTypeTiles.Add(_currentTiles[i]);
        }

        if (matchTypeTiles.Count <= 0)
        {
            int randAllIndex = UnityEngine.Random.Range(0, _currentTiles.Count);
            return _currentTiles[randAllIndex];
        }

        int matchRandIndex = UnityEngine.Random.Range(0, matchTypeTiles.Count);
        return matchTypeTiles[matchRandIndex];
    }

    /// <returns>
    /// pointer toggled tile
    /// </returns>
    public Tile Current_Tile()
    {
        for (int i = 0; i < _currentTiles.Count; i++)
        {
            if (_currentTiles[i].pointer.pointerDetected == false) continue;
            return _currentTiles[i];
        }
        return null;
    }


    public int Tile_Count(TileScrObj tileScrObj)
    {
        int count = 0;

        for (int i = 0; i < _currentTiles.Count; i++)
        {
            if (_currentTiles[i].data.tileScrObj != tileScrObj) continue;
            count++;
        }
        return count;
    }


    // Select
    public bool Tile_Selectable(Tile tile)
    {
        if (tile == null) return false;

        InGame_Manager manager = InGame_Manager.instance;

        Cursor cursor = manager.cursor;
        Tile playerTile = manager.player.movement.currentTile;

        return playerTile == tile || cursor.itemCursor.data != null && cursor.PointingTile_InRange(tile);
    }
    private bool Tile_Selectable(out Tile currentTile)
    {
        currentTile = Current_Tile();
        return Tile_Selectable(currentTile);
    }


    public void Hover_Tile()
    {
        OnTileHover?.Invoke(Current_Tile());
    }

    public void Select_Tile()
    {
        if (Tile_Selectable(out Tile currentTile) == false) return;

        OnTargetTileSelect?.Invoke(currentTile);
        OnTileSelect?.Invoke();
    }

    public void HoldSelect_Tile()
    {
        if (Tile_Selectable(out Tile currentTile) == false) return;

        OnTargetTileHoldSelect?.Invoke(currentTile);
        OnTileSelect?.Invoke();
    }

    public void RightSelect_Tile()
    {
        if (Tile_Selectable(out Tile currentTile) == false) return;

        OnTileRightSelect?.Invoke(currentTile);
        OnTileSelect?.Invoke();
    }


    // Update
    private void Load_SetSprites()
    {
        Vector2 generateStartPos = InGame_Manager.instance.worldMapGenerator.Generate_StartPosition();

        for (int i = 0; i < _currentTiles.Count; i++)
        {
            bool setOnBase = _currentTiles[i].transform.position.y <= -generateStartPos.y;
            _currentTiles[i].Update_SetSprite(setOnBase);
        }
    }

    private void Refresh_Toggles()
    {
        foreach (Tile tile in _currentTiles)
        {
            tile.Toggle_SelectPreview(Tile_Selectable(tile));
            tile.Toggle_SelectReady();
        }
    }
}