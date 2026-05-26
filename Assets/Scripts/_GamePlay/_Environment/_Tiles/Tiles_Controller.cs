using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles_Controller : MonoBehaviour, IItemsSource
{
    [Space(20)]
    [SerializeField] private TileState_VisualData[] _tileStateVisualDatas;


    private List<Tile> _currentTiles = new();
    public List<Tile> currentTiles => _currentTiles;

    private List<PlaceableItem> _placedItems = new();
    public List<PlaceableItem> placedItems => _placedItems;


    public Action<Tile> OnTileHover;
    public Action<Tile> OnTileHoldHover;

    public Action OnTileSelect;
    
    public Action<Tile> OnTargetTileSelect;
    public Action<Tile> OnTargetTileHoldSelect;
    public Action<Tile> OnTileRightSelect;

    public Action OnTileStatesTimeCount;


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
        manager.player.movement.tileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Refresh_Toggles);

        manager.time.UnRegister(ActionUpdateBus.StartUpdate, UpdateTiles_StateTimeCount);
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
        manager.player.movement.tileTracker.Register(ActionUpdateBus.AwakeUpdate, Refresh_Toggles);

        manager.time.Register(ActionUpdateBus.StartUpdate, UpdateTiles_StateTimeCount);
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
    public List<Tile> Current_Tiles(Item_ScrObj targetItem)
    {
        List<Tile> itemExistTiles = new();

        for (int i = 0; i < _currentTiles.Count; i++)
        {
            Tile currentTile = _currentTiles[i];

            if (currentTile.PlacedItem(targetItem) == null) continue;
            itemExistTiles.Add(currentTile);
        }
        return itemExistTiles;
    }

    public List<Tile> Current_EdgedTiles()
    {
        InGame_Manager manager = InGame_Manager.instance;

        Vector2 size = manager.worldMapGenerator.Converted_GenerateSize();
        Vector2 start = manager.worldMapGenerator.Generate_StartPosition();

        float minX = start.x;
        float maxX = start.x + size.x - 1;

        float maxY = start.y;
        float minY = start.y - size.y + 1;

        List<Tile> edgedTiles = new();

        for (int i = 0; i < _currentTiles.Count; i++)
        {
            Tile tile = _currentTiles[i];
            Vector2 pos = tile.transform.position;

            if (pos.x != minX && pos.x != maxX && pos.y != minY && pos.y != maxY) continue;
            edgedTiles.Add(tile);
        }
        return edgedTiles;
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


    // Visual Data
    public Sprite TileState_VisualSprite(TileState tileState)
    {
        for (int i = 0; i < _tileStateVisualDatas.Length; i++)
        {
            TileState_VisualData visualData = _tileStateVisualDatas[i];

            if (tileState != visualData.tileState) continue;
            return visualData.indicationSprite;
        }
        return null;
    }


    // Select
    public bool Tile_Pointable(Tile tile)
    {
        if (tile == null) return false;

        InGame_Manager manager = InGame_Manager.instance;
        Cursor cursor = manager.cursor;

        if (cursor.PointingTile_InRange(tile) == false) return false;
        return true;
    }
    public bool Tile_Selectable(Tile tile)
    {
        if (Tile_Pointable(tile) == false) return false;

        InGame_Manager manager = InGame_Manager.instance;

        if (manager.time.TimeUpdateActions_Running()) return false;
        if (manager.movements.AllMovements_Complete() == false) return false;

        Tile playerTile = manager.player.movement.tileTracker.data.CurrentTile();
        ItemData currentItemData = manager.cursor.itemCursor.data;

        if (currentItemData == null) return tile == playerTile;
        return currentItemData.itemScrObj.Select_Available(currentItemData, tile);
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
    public void HoldHover_Tile(bool hovering)
    {
        OnTileHoldHover?.Invoke(hovering ? Current_Tile() : null);
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
    private bool SetOn_MapBase(Tile tile)
    {
        Vector2 generateStartPos = InGame_Manager.instance.worldMapGenerator.Generate_StartPosition();
        return tile.transform.position.y <= -generateStartPos.y;
    }
    private void Load_SetSprites()
    {
        foreach (Tile tile in _currentTiles)
        {
            tile.Update_SetSprite(SetOn_MapBase(tile));
        }
    }

    private void Refresh_Toggles()
    {
        foreach (Tile tile in _currentTiles)
        {
            tile.Toggle_SelectPreview(Tile_Pointable(tile));
            tile.Toggle_SelectReady();
        }
    }
    private void Refresh_Toggles(Tile _)
    {
        Refresh_Toggles();
    }

    private void UpdateTiles_StateTimeCount()
    {
        StartCoroutine(StateTimeCount_TilesUpdateDelay());
    }
    private IEnumerator StateTimeCount_TilesUpdateDelay()
    {
        Time_Manager time = InGame_Manager.instance.time;
        while (time.TimeUpdateActions_Running()) yield return null;

        foreach (Tile tile in _currentTiles)
        {
            tile.data.Decrease_StateDatas();
        }
        OnTileStatesTimeCount?.Invoke();
    }


    // Placed Items
    public List<PlaceableItem> PlacedItems(Item_ScrObj targetItem)
    {
        List<PlaceableItem> placedItems = new();

        for (int i = 0; i < _placedItems.Count; i++)
        {
            PlaceableItem placedITem = _placedItems[i];

            if (targetItem != placedITem.data.itemScrObj) continue;
            placedItems.Add(placedITem);
        }
        return placedItems;
    }
}