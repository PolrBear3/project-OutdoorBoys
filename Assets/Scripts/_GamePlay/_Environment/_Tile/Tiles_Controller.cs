using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles_Controller : MonoBehaviour
{
    private List<Tile> _currentTiles = new();
    public List<Tile> currentTiles => _currentTiles;

    public Action<Tile> OnTileInteract;


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

        Time_Manager time = InGame_Manager.instance.time;

        time.OnNightPhase -= Update_Shadows;
        time.OnDayCountUpdate -= Update_Shadows;
    }


    // Data
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


    private void Set_Data()
    {
        Time_Manager time = InGame_Manager.instance.time;

        time.OnNightPhase += Update_Shadows;
        time.OnDayCountUpdate += Update_Shadows;
    }

    private void Update_DataSprites()
    {
        Vector2 generateStartPos = InGame_Manager.instance.tileGenerator.Generate_StartPosition();

        for (int i = 0; i < _currentTiles.Count; i++)
        {
            bool setOnBase = _currentTiles[i].transform.position.y <= -generateStartPos.y;
            _currentTiles[i].Update_SetSprite(setOnBase);
        }
    }


    // Shadow
    private void Update_Shadows()
    {
        bool isNight = InGame_Manager.instance.time.Is_Night();
        
        for (int i = 0; i < _currentTiles.Count; i++)
        {
            currentTiles[i].Toggle_Shadow(isNight);
        }
    }
}