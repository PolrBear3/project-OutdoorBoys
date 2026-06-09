using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneCollection_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemsSource_Manager _itemSourceManager;
    [SerializeField] private ItemSlot_Manager _collectableSlotsManager;


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
    }


    // Data
    private void Set_Data()
    {
        Inventory_Manager inventory = InGame_Manager.instance.inventory;

        inventory.OnItemAdded += UpdateCollectables_toSlots;
        inventory.OnToggle += UpdateCollectables_toSlots;
    }


    // Visuals
    private void UpdateCollectables_toSlots()
    {
        InGame_Manager manager = InGame_Manager.instance;
        if (manager.inventory.Toggled()) return;

        BoneCollectable_ScrObj[] collectableBones = manager.worldMapGenerator.currentWorldMap.boneCollectables;

        for (int i = 0; i < collectableBones.Length; i++)
        {
            Item_ScrObj collectableBone = collectableBones[i];

            List<ItemData> inventoryItemDatas = _itemSourceManager.ItemDatas();
            bool collected = false;

            for (int j = 0; j < inventoryItemDatas.Count; j++)
            {
                if (inventoryItemDatas[j].itemScrObj != collectableBone) continue;

                collected = true;
                break;
            }

            ItemSlot slot = _collectableSlotsManager.slots[i];
            slot.Set_Data(collected ? new ItemData(collectableBone, 1) : null);
        }
        _collectableSlotsManager.Update_Visuals();
    }
    private void UpdateCollectables_toSlots(bool inventoryToggled)
    {
        if (inventoryToggled) return;

        UpdateCollectables_toSlots();
    }
}
