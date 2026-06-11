using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneCollection_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemsSource_Manager _itemSourceManager;

    [Space(20)]
    [SerializeField] private ItemSlot_Manager _collectableSlotsManager;
    [SerializeField] private ItemInfo_Controller _hoverInfo;


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
        BoneCollectable_ScrObj[] collectableBones = InGame_Manager.instance.worldMapGenerator.currentWorldMap.boneCollectables;

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

        BoneCollectable_ScrObj[] collectableBones = manager.worldMapGenerator.currentWorldMap.boneCollectables;

        for (int i = 0; i < collectableBones.Length; i++)
        {
            ItemSlot slot = _collectableSlotsManager.slots[i];

            slot.Set_Data(new ItemData(collectableBones[i], 1));
            slot.Toggle_Transparency(Bone_Collected(slot.data) == false);
        }
        _collectableSlotsManager.Update_Visuals();
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

        hoveringSlot.data.Update_CurrentAmount(_itemSourceManager.ItemData_Count(boneItem));
        _hoverInfo.Update_HoveringItemInfo(hoveringSlot, Bone_Collected(hoveringSlotData) ? boneItem.collectedDescription : boneItem.description);
    }
    private void UpdateInfo_HoveringCollectable()
    {
        UpdateInfo_HoveringCollectable(_collectableSlotsManager.hoveringSlot);
    }
}
