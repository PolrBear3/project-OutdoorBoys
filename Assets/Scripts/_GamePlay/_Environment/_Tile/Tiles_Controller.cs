using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles_Controller : MonoBehaviour
{
    private List<Tile> _currentTiles = new();
    public List<Tile> currentTiles => _currentTiles;


    public Action<Tile> OnTileSelect;
    public Action<Tile> OnTileHoldSelect;

    public Action OnTileSelectComplete;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.Register(EventBus.StartLoad, Update_DataSprites);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Update_DataSprites);

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick -= Select_Tile;
        input.OnHoldLeftClick -= HoldSelect_Tile;

        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.OnNightPhase -= Update_Shadows;
        time.OnDayCountUpdate -= Update_Shadows;

        manager.cursor.OnTilePointRangeUpdate -= Update_PointerToggles;
        manager.player.movement.OnMovement -= Update_PointerToggles;
    }


    // Data
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick += Select_Tile;
        input.OnHoldLeftClick += HoldSelect_Tile;

        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.OnNightPhase += Update_Shadows;
        time.OnDayCountUpdate += Update_Shadows;

        manager.cursor.OnTilePointRangeUpdate += Update_PointerToggles;
        manager.player.movement.OnMovement += Update_PointerToggles;
    }


    public Tile Current_Tile(Vector2 generatedPos)
    {
        for (int i = 0; i < _currentTiles.Count; i++)
        {
            if ((Vector2)_currentTiles[i].transform.position != generatedPos) continue;
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
            if (_currentTiles[i].pointerToggled == false) continue;
            return _currentTiles[i];
        }
        return null;
    }


    // Select
    public void Select_Tile()
    {
        Tile pointingTile = Current_Tile();
        if (pointingTile == null) return;

        OnTileSelect?.Invoke(pointingTile);
        OnTileSelectComplete?.Invoke();
    }

    public void HoldSelect_Tile()
    {
        Tile pointingTile = Current_Tile();
        if (pointingTile == null) return;

        OnTileHoldSelect?.Invoke(pointingTile);
        OnTileSelectComplete?.Invoke();
    }


    // Update
    private void Update_DataSprites()
    {
        Vector2 generateStartPos = InGame_Manager.instance.tileGenerator.Generate_StartPosition();

        for (int i = 0; i < _currentTiles.Count; i++)
        {
            bool setOnBase = _currentTiles[i].transform.position.y <= -generateStartPos.y;
            _currentTiles[i].Update_SetSprite(setOnBase);
        }
    }

    private void Update_Shadows()
    {
        bool isNight = InGame_Manager.instance.time.Is_Night();

        foreach (Tile tile in _currentTiles)
        {
            tile.Toggle_Shadow(isNight);
        }
    }

    private void Update_PointerToggles()
    {
        foreach (Tile tile in _currentTiles)
        {
            tile.Toggle_Pointer();
        }
    }
}