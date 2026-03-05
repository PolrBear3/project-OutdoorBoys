using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCrafting_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemSlot_Manager _slotManager;
    [SerializeField] private Image _togglePanel;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }
    
    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Input_Controller input = Input_Controller.instance;
        input.OnLeftClick -= Craft_Item;
    }


    // Component
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;
        input.OnLeftClick += Craft_Item;
    }


    // Craft
    private List<ItemData> AllCurrent_ItemDatas()
    {
        InGame_Manager manager = InGame_Manager.instance;

        ItemData carryItemData = manager.cursor.itemCursor.data;

        Inventory_Manager inventory = manager.inventory;
        List<ItemData> inventoryDatas = inventory.Toggled() ? inventory.slotManager.Slot_ItemDatas() : null;

        Tile playerTile = manager.player.movement.currentTile;
        List<ItemData> tilePlacedDatas = playerTile.Placed_ItemDatas();

        List<ItemData> allCurrentDatas = new();

        if (carryItemData != null) allCurrentDatas.Add(carryItemData);
        if (inventoryDatas != null) allCurrentDatas.AddRange(inventoryDatas);
        if (tilePlacedDatas != null) allCurrentDatas.AddRange(tilePlacedDatas);

        return allCurrentDatas;
    }
    
    public void Update_CraftItems()
    {
        Item_ScrObj[] allItems = Data_Manager.instance.allItems;

        List<ItemData> currentItemDatas = new(AllCurrent_ItemDatas());
        List<ItemData> craftAvailableItemDatas = new();

        for (int i = 0; i < allItems.Length; i++)
        {
            Item_ScrObj craftItem = allItems[i];
            int craftCount = craftItem.Available_CraftCount(currentItemDatas);
            
            if (craftCount <= 0) continue;
            craftAvailableItemDatas.Add(new(craftItem, craftCount));
        }
        craftAvailableItemDatas.Sort((x, y) => y.amount.CompareTo(x.amount));

        List<ItemSlot> craftSlots = _slotManager.slots;

        for (int i = 0; i < craftSlots.Count; i++)
        {
            ItemSlot slot = craftSlots[i];

            if (i >= craftAvailableItemDatas.Count)
            {
                slot.Clear_Data();
                continue;
            }
            slot.Set_Data(craftAvailableItemDatas[i]);
        }

        _slotManager.Update_Visuals();
    }

    private void Craft_Item()
    {
        ItemSlot hoveringSlot = _slotManager.hoveringSlot;
        if (hoveringSlot == null) return;

        List<ItemData> currentDatas = AllCurrent_ItemDatas();

        Item_ScrObj craftItem = hoveringSlot.data.itemScrObj;
        List<ItemData> craftIngredientDatas = new(craftItem.Item_IngredientDatas());

        for (int i = 0; i < craftIngredientDatas.Count; i++)
        {
            int targetCount = craftIngredientDatas[i].amount;

            for (int j = 0; j < currentDatas.Count; j++)
            {
                if (craftIngredientDatas[i].itemScrObj != currentDatas[j].itemScrObj) continue;

                int consumeAmount = Mathf.Min(currentDatas[j].amount, targetCount);

                currentDatas[j].Update_CurrentAmount(currentDatas[j].amount - consumeAmount);
                targetCount -= consumeAmount;

                if (targetCount <= 0) break;
            }
        }

        InGame_Manager manager = InGame_Manager.instance;
        ItemCursor itemCursor = manager.cursor.itemCursor;

        Inventory_Manager inventory = manager.inventory;
        ItemSlot_Manager inventorySlotManager = inventory.slotManager;

        // itemCursor update
        itemCursor.Set_Data(itemCursor.data);
        itemCursor.Update_Visuals();

        // placed items update
        manager.player.movement.currentTile.RemoveUpdate_PlacedItems();

        // inventory update
        inventorySlotManager.Refresh_Datas();

        // craft item
        int craftAmount = craftItem.itemType == ItemType.use ? craftItem.maxAmount : 1;

        inventory.Add_ItemData(new(craftItem, craftAmount));
        inventorySlotManager.Update_Visuals();

        Update_CraftItems();
    }
}