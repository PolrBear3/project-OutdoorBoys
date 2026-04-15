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
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Input_Controller input = Input_Controller.instance;

        input.OnMovement -= MoveTo_Tile;

        Time_Manager time = InGame_Manager.instance.time;
        Movement_Controller playerMovement = _controller.movement;

        playerMovement.OnMovement -= time.Count_Time;
        playerMovement.OnMovementActive -= Update_MovementAnimation;

        time.OnTimeCount -= Charge_Stamina;
    }


    // IItemsSource
    public IEnumerable<ItemData> ItemDatas()
    {
        Tile currentTile = _controller.movement.tileTrackerData.CurrentTile();
        List<ItemData> currentTileItemDatas = currentTile.Placed_ItemDatas();

        foreach (ItemData data in currentTileItemDatas)
        {
            yield return data;
        }
    }

    public int RemoveItem(Item_ScrObj updateItem, int removeAmount)
    {
        Tile currentTile = _controller.movement.tileTrackerData.CurrentTile();
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
        Tile playerTile = _controller.movement.tileTrackerData.CurrentTile();
        int placeAmount = Mathf.Min(addAmount, playerTile.ItemPlace_AvailableCount(addItem));

        playerTile.Set_Item(new(addItem, placeAmount));
        return placeAmount;
    }


    // Data
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;

        input.OnMovement += MoveTo_Tile;

        Time_Manager time = InGame_Manager.instance.time;
        Movement_Controller playerMovement = _controller.movement;

        playerMovement.OnMovement += time.Count_Time;
        playerMovement.OnMovementActive += Update_MovementAnimation;

        time.OnTimeCount += Charge_Stamina;
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

    private void Update_MovementAnimation(bool isMoving)
    {
        AnimationPlayer animPlayer = _controller.animationPlayer;

        int animIndexNum = isMoving ? 1 : 0;
        animPlayer.Play(animIndexNum);
    }


    // Movement & Stamina
    public int Movement_StaminaValue()
    {
        InGame_Manager manager = InGame_Manager.instance;

        ItemData currentItem = manager.cursor.itemCursor.data;
        bool hasInventoryBagpack = currentItem != null && currentItem.itemScrObj == _controller.inventoryBagpack;

        int currentInventoryWeight = hasInventoryBagpack ? manager.inventory.slotManager.Total_ItemWeight() : 0;
        int currentItemWeight = currentItem != null ? currentItem.Item_Weight() + currentInventoryWeight : 0;

        return Mathf.Max(1, currentItemWeight);
    }

    private bool MoveAvailable_UpdateStamina()
    {
        InGame_Manager manager = InGame_Manager.instance;
        if (manager.movements.AllMovements_Complete() == false) return false;

        Player_Controller player = manager.player;
        player.Update_CurrentStamina(player.data.currentStamina - Mathf.Max(1, Movement_StaminaValue()));
        
        return true;
    }
    private void MoveTo_Tile(Vector2 direction)
    {
        OnMoveAvailableCheck?.Invoke();
        if (MoveAvailable_UpdateStamina() == false) return;

        _controller.movement.MoveTo_Tile(direction);
    }

    private void Charge_Stamina(int _)
    {
        Time_Manager time = InGame_Manager.instance.time;
        if (time.timeTikCoroutine == null) return;

        int chargeValue = 1; // move this to upgrade settings
        _controller.Update_CurrentStamina(_controller.data.currentStamina + chargeValue);
    }
}