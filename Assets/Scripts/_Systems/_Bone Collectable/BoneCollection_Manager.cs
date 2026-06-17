using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoneCollectable_Data
{
    [SerializeField] private Sprite _completedSprite;
    public Sprite completedSprite => _completedSprite;
    
    [SerializeField] private BoneCollectable_ScrObj[] _collectables;
    public BoneCollectable_ScrObj[] collectables => _collectables;
}

public class BoneCollection_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemsSource_Manager _itemSourceManager;

    [Space(20)]
    [SerializeField] private ItemSlot_Manager _collectableSlotsManager;
    [SerializeField] private ItemInfo_Controller _hoverInfo;

    [Space(20)]
    [SerializeField] private ItemSlot _completedSlot;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);


        Inventory_Manager inventory = InGame_Manager.instance.inventory;

        inventory.OnItemAdded -= UpdateCollectables_toSlots;
        inventory.OnToggle -= UpdateCollectables_toSlots;

        inventory.OnItemAdded -= UpdateInfo_HoveringCollectable;

        _collectableSlotsManager.OnSlotHover -= _hoverInfo.Toggle_ItemInfoPanel;
        _collectableSlotsManager.OnSlotHover -= UpdateInfo_HoveringCollectable;
    }


    // Data
    private void Set_Data()
    {
        Inventory_Manager inventory = InGame_Manager.instance.inventory;

        inventory.OnItemAdded += UpdateCollectables_toSlots;
        inventory.OnToggle += UpdateCollectables_toSlots;

        inventory.OnItemAdded += UpdateInfo_HoveringCollectable;

        _collectableSlotsManager.OnSlotHover += _hoverInfo.Toggle_ItemInfoPanel;
        _collectableSlotsManager.OnSlotHover += UpdateInfo_HoveringCollectable;


        _hoverInfo.Toggle_ItemInfoPanel(_collectableSlotsManager.hoveringSlot);
    }

    private BoneCollectable_ScrObj BoneCollectable_Item(Item_ScrObj item)
    {
        BoneCollectable_ScrObj[] collectableBones = InGame_Manager.instance.worldMapGenerator.currentWorldMap.boneCollectableData.collectables;

        for (int i = 0; i < collectableBones.Length; i++)
        {
            BoneCollectable_ScrObj collectableBone = collectableBones[i];

            if (item != collectableBone) continue;
            return collectableBone;
        }
        return null;
    }
    private bool Bone_Collected(ItemData checkData)
    {
        BoneCollectable_ScrObj boneItem = BoneCollectable_Item(checkData.itemScrObj);

        if (boneItem == null) return false;
        if (_itemSourceManager.ItemData_Count(boneItem) < boneItem.maxAmount) return false;

        return true;
    }


    // Visuals
    private void UpdateCollectables_toSlots()
    {
        InGame_Manager manager = InGame_Manager.instance;
        if (manager.inventory.Toggled()) return;

        BoneCollectable_Data data = manager.worldMapGenerator.currentWorldMap.boneCollectableData;
        BoneCollectable_ScrObj[] collectableBones = data.collectables;
        
        int collectCount = 0;

        for (int i = 0; i < collectableBones.Length; i++)
        {
            ItemSlot slot = _collectableSlotsManager.slots[i];
            Item_ScrObj collectableBone = collectableBones[i];

            slot.Set_Data(new ItemData(collectableBone, 1));

            bool boneCollected = Bone_Collected(slot.data);
            slot.Toggle_Transparency(boneCollected == false);

            if (boneCollected == false) continue;
            collectCount++;
        }

        bool collectCompleted = collectCount >= collectableBones.Length;

        _collectableSlotsManager.Toggle_Slots(!collectCompleted);
        _completedSlot.gameObject.SetActive(collectCompleted);

        if (collectCompleted == false)
        {
            _collectableSlotsManager.Update_Visuals();
            return;
        }
        _completedSlot.itemImage.sprite = data.completedSprite;
    }
    private void UpdateCollectables_toSlots(bool inventoryToggled)
    {
        if (inventoryToggled) return;

        UpdateCollectables_toSlots();
    }

    private void UpdateInfo_HoveringCollectable(ItemSlot hoveringSlot)
    {
        InGame_Manager manager = InGame_Manager.instance;
        if (manager.inventory.Toggled()) return;

        if (hoveringSlot == null || hoveringSlot.data == null) return;
        ItemData hoveringSlotData = hoveringSlot.data;

        BoneCollectable_ScrObj boneItem = BoneCollectable_Item(hoveringSlotData.itemScrObj);
        if (boneItem == null) return;

        string description = Bone_Collected(hoveringSlotData) ? boneItem.collectedDescription : boneItem.description;

        hoveringSlot.data.Update_CurrentAmount(_itemSourceManager.ItemData_Count(boneItem));
        _hoverInfo.Update_HoveringItemInfo(hoveringSlot, description);
    }
    private void UpdateInfo_HoveringCollectable()
    {
        UpdateInfo_HoveringCollectable(_collectableSlotsManager.hoveringSlot);
    }
}