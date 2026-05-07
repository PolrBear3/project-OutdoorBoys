using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Interaction : MonoBehaviour, IItemsSource, IItemsSourceRemove, IItemsSourceAdd
{
    [SerializeField] private Player_Controller _controller;

    [Space(20)]
    [SerializeField] private SpriteRenderer _indicationIcon;
    public SpriteRenderer indicationIcon => _indicationIcon;

    private GameObject _currentItemPrefab;
    public GameObject currentItemPrefab => _currentItemPrefab;

    public Action OnMoveAvailableCheck;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void Start()
    {
        Update_IndicationIcon(null);
    }

    private void OnDestroy()
    {
        _controller.movement.tileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, InteractUpdate_Stamina);
        InGame_Manager.instance.time.UnRegister(ActionUpdateBus.AwakeUpdate, MaxUpdate_Stamina);
    }


    // IItemsSource
    public IEnumerable<ItemData> ItemDatas()
    {
        Tile currentTile = _controller.movement.tileTracker.data.CurrentTile();
        List<ItemData> currentTileItemDatas = currentTile.Placed_ItemDatas();

        foreach (ItemData data in currentTileItemDatas)
        {
            yield return data;
        }
    }

    public int RemoveItem(Item_ScrObj updateItem, int removeAmount)
    {
        Tile currentTile = _controller.movement.tileTracker.data.CurrentTile();
        List<PlaceableItem> placedItems = currentTile.PlacedItems(updateItem);

        if (placedItems.Count <= 0) return 0;

        bool isUseItem = updateItem.itemType == ItemType.use;
        int totalRemoveCount = 0;

        for (int i = 0; i < placedItems.Count; i++)
        {
            PlaceableItem placedItem = placedItems[i];
            ItemData placedData = placedItem.data;

            int placedAmount = placedData.amount;

            if (isUseItem == false)
            {
                int removeUpdateAmount = Mathf.Min(placedAmount, removeAmount);

                placedData.Update_CurrentAmount(placedAmount - removeUpdateAmount);

                removeAmount -= removeUpdateAmount;
                totalRemoveCount += removeUpdateAmount;

                if (removeAmount <= 0) break;
                continue;
            }

            // counts only max amount useable items as 1 (change for gameplay)
            if (placedAmount < updateItem.maxAmount) continue;

            placedItem.data.Update_CurrentAmount(0);

            removeAmount--;
            totalRemoveCount++;

            if (removeAmount <= 0) break;
        }

        currentTile.Remove_EmptyPlacedItems();
        return totalRemoveCount;
    }

    public int AddItem(Item_ScrObj addItem, int addAmount)
    {
        Tile playerTile = _controller.movement.tileTracker.data.CurrentTile();
        int placeAmount = Mathf.Min(addAmount, playerTile.ItemPlace_AvailableCount(addItem));

        playerTile.Set_Item(new(addItem, placeAmount));
        return placeAmount;
    }


    // Data
    private void Set_Data()
    {
        _controller.movement.tileTracker.Register(ActionUpdateBus.AwakeUpdate, InteractUpdate_Stamina);
        InGame_Manager.instance.time.Register(ActionUpdateBus.AwakeUpdate, MaxUpdate_Stamina);
    }

    public void Load_ItemPrefab(GameObject itemPrefab)
    {
        Destroy(_currentItemPrefab);
        _currentItemPrefab = null;

        if (itemPrefab == null) return;

        _currentItemPrefab = Instantiate(itemPrefab, transform);
    }


    // Visuals
    public void Update_IndicationIcon(Sprite iconSprite)
    {
        bool updateAvailable = iconSprite != null;
        _indicationIcon.gameObject.SetActive(updateAvailable);

        if (updateAvailable == false) return;
        _indicationIcon.sprite = iconSprite;
    }


    // Stamina
    public int Current_StaminaValue()
    {
        InGame_Manager manager = InGame_Manager.instance;

        ItemData currentItem = manager.cursor.itemCursor.data;
        Item_ScrObj inventoryBagpack = _controller.inventoryBagpack;

        bool hasInventoryBagpack = currentItem != null && currentItem.itemScrObj == inventoryBagpack;

        int currentInventoryWeight = hasInventoryBagpack ? inventoryBagpack.itemWeight : 0;
        int currentItemWeight = currentItem != null ? currentItem.Item_Weight() + currentInventoryWeight : 0;

        return Mathf.Max(1, currentItemWeight);
    }

    public bool Has_Stamina()
    {
        return _controller.data.currentStamina - Current_StaminaValue() >= 0;
    }


    public void InteractUpdate_Stamina()
    {
        _controller.Update_CurrentStamina(_controller.data.currentStamina - Mathf.Max(1, Current_StaminaValue()));
    }
    private void InteractUpdate_Stamina(Tile _)
    {
        InteractUpdate_Stamina();
    }

    private void MaxUpdate_Stamina()
    {
        _controller.Update_CurrentStamina(_controller.data.maxStamina);
    }
}