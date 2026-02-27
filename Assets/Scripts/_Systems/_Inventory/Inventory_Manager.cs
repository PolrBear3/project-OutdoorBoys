using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_Manager : MonoBehaviour
{
    [SerializeField] private Item_ScrObj _inventoryBagpack;
    
    [Space(20)]
    [SerializeField] private Image _togglePanel;

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
    private void Toggle_Update(Tile selectedTile)
    {
        Item_ScrObj currentItem = InGame_Manager.instance.cursor.itemCursor.itemData?.itemScrObj;
        bool itemPlaced = currentItem == null || currentItem != _inventoryBagpack;

        _togglePanel.gameObject.SetActive(itemPlaced);
    }
    private void Toggle_Update()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;
        List<PlaceableItem> placedItems = playerTile.placedItems;

        GameObject togglePanel = _togglePanel.gameObject;

        for (int i = 0; i < placedItems.Count; i++)
        {
            if (placedItems[i].data.itemScrObj != _inventoryBagpack) continue;

            togglePanel.SetActive(true);
            return;
        }
        togglePanel.SetActive(false);
    }
}
