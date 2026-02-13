using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles_Controller : MonoBehaviour
{
    private List<Tile> _currentTiles = new();
    public List<Tile> currentTiles => _currentTiles;


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

        InGame_Manager.instance.time.OnNightTime -= Toggle_Shadows;
    }


    // Data
    public Tile Positioned_Tile(Vector2 generatedPos)
    {
        for (int i = 0; i < _currentTiles.Count; i++)
        {
            if ((Vector2)_currentTiles[i].transform.position != generatedPos) continue;
            return _currentTiles[i];
        }
        return null;
    }


    private void Set_Data()
    {
        InGame_Manager.instance.time.OnNightTime += Toggle_Shadows;
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
    private void Toggle_Shadows(bool toggle)
    {
        for (int i = 0; i < _currentTiles.Count; i++)
        {
            currentTiles[i].Toggle_Shadow(toggle);
        }
    }
}