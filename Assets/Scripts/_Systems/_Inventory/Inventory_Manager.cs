using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private InventorySlot[] _slots;
    public InventorySlot[] slots => _slots;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.Register(EventBus.StartLoad, Toggle_Update);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Toggle_Update);

        InGame_Manager manager = InGame_Manager.instance;
        manager.player.movement.OnMovement -= Toggle_Update;

        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileSelect -= Toggle_Update;
        tilesController.OnTileHoldSelect -= Toggle_Update;
    }


    // Data
    public void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;
        manager.player.movement.OnMovement += Toggle_Update;

        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileSelect += Toggle_Update;
        tilesController.OnTileHoldSelect += Toggle_Update;
    }


    // Main
    private void Toggle_Update()
    {
        Debug.Log("toggle update");
    }
    private void Toggle_Update(Tile selectedTile)
    {
        Debug.Log("toggle update : " + selectedTile);
    }
}
