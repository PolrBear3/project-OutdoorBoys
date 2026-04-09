using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Interaction : MonoBehaviour, IItemsSource, IItemsSourceRemove, IItemsSourceAdd
{
    [SerializeField] private Player_Controller _controller;

    [Space(20)]
    [SerializeField] private SpriteRenderer _indicationIcon;
    public SpriteRenderer indicationIcon => _indicationIcon;

    [Space(20)]
    [SerializeField][Range(0, 10)] private int _movementTimeCost;


    private GameObject _currentItemPrefab;
    public GameObject currentItemPrefab => _currentItemPrefab;


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
        Movement_Controller playerMovement = _controller.movement;

        input.OnMovement -= playerMovement.MoveTo_Tile;

        playerMovement.OnMovement -= InGame_Manager.instance.time.Count_Time;
        playerMovement.OnMovementStated -= Update_MovementAnimation;
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
        Movement_Controller playerMovement = _controller.movement;

        input.OnMovement += playerMovement.MoveTo_Tile;

        playerMovement.OnMovement += InGame_Manager.instance.time.Count_Time;
        playerMovement.OnMovementStated += Update_MovementAnimation;
    }

    public void Update_IndicationIcon(Sprite iconSprite)
    {
        bool updateAvailable = iconSprite != null;
        _indicationIcon.gameObject.SetActive(updateAvailable);

        if (updateAvailable == false) return;
        _indicationIcon.sprite = iconSprite;
    }


    // Maint
    private void Update_MovementAnimation(bool isMoving)
    {
        AnimationPlayer animPlayer = _controller.animationPlayer;

        int animIndexNum = isMoving ? 1 : 0;
        animPlayer.Play(animIndexNum);
    }

    public void Update_MovementTimeCost()
    {
        InGame_Manager manager = InGame_Manager.instance;

        ItemData currentItem = manager.cursor.itemCursor.data;
        bool hasInventoryBagpack = currentItem != null && currentItem.itemScrObj == _controller.inventoryBagpack;

        int currentInventoryWeight = hasInventoryBagpack ? manager.inventory.slotManager.Total_ItemWeight() : 0;
        int currentItemWeight = currentItem != null ? currentItem.Item_Weight() + currentInventoryWeight : 0;

        int timeCost = Mathf.Max(1, _movementTimeCost + currentItemWeight * _movementTimeCost);
        manager.time.Track_TimeCountData(new(this, timeCost));
    }

    public void Load_ItemPrefab(GameObject itemPrefab)
    {
        Destroy(_currentItemPrefab);
        _currentItemPrefab = null;

        if (itemPrefab == null) return;

        _currentItemPrefab = Instantiate(itemPrefab, transform);
    }
}