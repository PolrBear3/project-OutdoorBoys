using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Reward_ItemData
{
    [SerializeField] private Item_ScrObj _rewardItem;
    public Item_ScrObj rewardItem => _rewardItem;

    [Space(10)]
    [SerializeField][Range(0, 100)] private int _minAmount;
    public int minAmount => _minAmount;

    [SerializeField][Range(0, 100)] private int _maxAmount;
    public int maxAmount => _maxAmount;


    public ItemData RewardData()
    {
        if (_rewardItem == null) return null;

        int randAmount = Random.Range(_minAmount, _maxAmount + 1);
        int rewardAmount = _rewardItem.itemType == ItemType.use ? Mathf.Min(randAmount, _rewardItem.maxAmount) : randAmount;

        return new(_rewardItem, rewardAmount);
    }
}

public class Reward_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private PanelToggle_AnimationController _panelToggleController;

    [Space(10)]
    [SerializeField] private ItemSlot_Manager _itemSlotManager;
    [SerializeField] private ItemInfo_Controller _itemInfo;

    [Space(10)]
    [SerializeField] private ItemsSource_Manager _itemSource;


    [Space(20)]
    [SerializeField] private Reward_ItemData[] _rewardItemDatas;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }
    
    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        InGame_Manager.instance.time.OnRewardTargetTime -= Update_RewardItems;

        _itemSlotManager.OnSlotHover -= _itemInfo.Toggle_ItemInfoPanel;
        _itemSlotManager.OnSlotHover -= _itemInfo.Update_HoveringItemInfo;

        _itemSlotManager.OnSlotSelect -= Select_RewardItem;
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager.instance.time.OnRewardTargetTime += Update_RewardItems;

        _itemSlotManager.OnSlotHover += _itemInfo.Toggle_ItemInfoPanel;
        _itemSlotManager.OnSlotHover += _itemInfo.Update_HoveringItemInfo;

        _itemSlotManager.OnSlotSelect += Select_RewardItem;

        _panelToggleController.Toggle(false);
        _itemInfo.Toggle_ItemInfoPanel(null);
    }


    // Main
    private ItemData Reward_ItemData()
    {
        List<Reward_ItemData> rewardDatas = new();

        foreach (Reward_ItemData data in _rewardItemDatas)
        {
            rewardDatas.Add(data);
        }
        
        List<ItemData> currentRewards = new(_itemSlotManager.Slot_ItemDatas());

        for (int i = rewardDatas.Count - 1; i >= 0 ; i--)
        {
            int currentRewardsCount = currentRewards.Count;
            if (currentRewardsCount <= 0) break;

            for (int j = 0; j < currentRewardsCount; j++)
            {
                if (rewardDatas[i].rewardItem != currentRewards[j].itemScrObj) continue;
                rewardDatas.RemoveAt(i);
                break;
            }
        }
        return rewardDatas.Count > 0 ? rewardDatas[Random.Range(0, rewardDatas.Count)].RewardData() : null;
    }

    private void Update_RewardItems()
    {
        _panelToggleController.Toggle(true);
        _itemSlotManager.Clear_Datas();

        List<ItemSlot> slots = _itemSlotManager.slots;

        foreach (ItemSlot slot in slots)
        {
            slot.Set_Data(Reward_ItemData());
        }
        _itemSlotManager.Update_Visuals();
    }


    private List<IItemsSourceAdd> AddItems_Source()
    {
        Inventory_Manager inventory = InGame_Manager.instance.inventory;
        bool includeInventory = inventory.Toggled();

        List<IItemsSourceAdd> itemsSources = new(_itemSource.itemsAddSources);
        if (includeInventory) return itemsSources;

        if (inventory is IItemsSourceAdd inventorySource) itemsSources.Remove(inventorySource);
        return itemsSources;
    }

    private void Select_RewardItem(ItemSlot selectedSlot)
    {  
        if (selectedSlot.data == null) return;

        ItemData selectedRewardData = selectedSlot.data;
        if (selectedRewardData == null) return;

        List<IItemsSourceAdd> rewardSources = AddItems_Source();
        _itemSource.AddItem(rewardSources, selectedRewardData.itemScrObj, selectedRewardData.amount);

        _panelToggleController.Toggle(false);
        _itemInfo.Toggle_ItemInfoPanel(null);
    }
}
