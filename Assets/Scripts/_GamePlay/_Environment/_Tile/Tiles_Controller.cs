using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Vector2 _shadowDirection;

    private List<Tile> _currentTiles = new();
    public List<Tile> currentTiles => _currentTiles;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Update_DataSprites);
        EventBus_Manager.Register(EventBus.StartLoad, Toggle_Shadows);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Update_DataSprites);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Toggle_Shadows);
    }


    // Data
    private Tile Positioned_Tile(Vector2 generatedPos)
    {
        for (int i = 0; i < _currentTiles.Count; i++)
        {
            if ((Vector2)_currentTiles[i].transform.position != generatedPos) continue;
            return _currentTiles[i];
        }
        return null;
    }

    private List<Tile> Grid_Tiles(bool isRow, int gridNum)
    {
        Tile_Generator tileGenerator = InGame_Manager.instance.tileGenerator;
        
        Vector2 generateSize = tileGenerator.Converted_GenerateSize();

        Vector2 searchPos = tileGenerator.Generate_StartPosition();
        Vector2 searchDirection = isRow ? new(1, 0) : new(0, -1);

        List<Tile> gridTiles = new();
        int tileCount = isRow ? (int)generateSize.x : (int)generateSize.y;

        for (int i = 0; i < tileCount; i++)
        {
            gridTiles.Add(Positioned_Tile(searchPos));
            searchPos += searchDirection;
        }

        return gridTiles;
    }


    // Data Update
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
    private void Toggle_Shadows() // toggles first top column rows of tiles
    {
        int gridNum = (int)InGame_Manager.instance.tileGenerator.Generate_StartPosition().x;
        List<Tile> toggleTiles = Grid_Tiles(true, gridNum);

        for (int i = 0; i < toggleTiles.Count; i++)
        {
            toggleTiles[i].Toggle_Shadow(true);
        }
    }
    private void Toggle_Shadows(bool toggle)
    {

    }
}