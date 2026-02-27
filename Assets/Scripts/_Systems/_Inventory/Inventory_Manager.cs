using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_Manager : MonoBehaviour
{
    [SerializeField] private Item_ScrObj _inventoryBagpack;

    [Space(20)]
    [SerializeField] private Image _togglePanel;

    [SerializeField] private InventorySlot[] _slots;
    public InventorySlot[] slots => _slots;


    [HideInInspector] public Item_ScrObj loadItem;
    [HideInInspector][Range(0, 100)] public int loadItemAmount;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.Register(EventBus.SubLoad, Toggle_Update);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.UnRegister(EventBus.SubLoad, Toggle_Update);

        InGame_Manager manager = InGame_Manager.instance;

        manager.player.movement.OnMovement -= Toggle_Update;
        manager.tilesController.OnTileSelectComplete -= Toggle_Update;
    }


    // Data
    public void Set_Data()
    {
        Load_ItemData(new(loadItem, loadItemAmount));
        Load_AllSlots();

        InGame_Manager manager = InGame_Manager.instance;

        manager.player.movement.OnMovement += Toggle_Update;
        manager.tilesController.OnTileSelectComplete += Toggle_Update;
    }


    // Main
    private void Toggle_Update()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;

        _togglePanel.gameObject.SetActive(playerTile.Placed_ItemCount(_inventoryBagpack) > 0);
    }


    private List<InventorySlot> EmptySlots()
    {
        List<InventorySlot> emptySlots = new();

        for (int i = 0; i < _slots.Length; i++)
        {
            InventorySlot slot = _slots[i];

            if (slot == null)
            {
                Debug.Log("Inspector Slot Field Empty at Index : " + i);
                return null;
            }

            if (slot.data == null) emptySlots.Add(slot);
        }
        return emptySlots;
    }


    public bool Load_ItemData(ItemData addData)
    {
        if (addData == null) return false;
        Item_ScrObj dataItem = addData.itemScrObj;

        int maxAmount = dataItem.maxAmount;
        if (maxAmount <= 0) return false;

        Dictionary<InventorySlot, int> slotLoadDatas = new();
        int loadCount = addData.amount;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
            {
                Debug.Log("Inspector Slot Field Empty at Index : " + i);
                return false;
            }

            InventorySlot slot = _slots[i];
            ItemData slotData = slot.data;

            if (slotData?.itemScrObj != dataItem) continue;
            if (slotData.amount >= maxAmount) continue;

            int slotSpace = maxAmount - slotData.amount;
            int loadAmount = Mathf.Min(slotSpace, loadCount);

            slotLoadDatas[slot] = loadAmount;
            loadCount -= loadAmount;

            if (loadCount <= 0)
            {
                Load_AllSlots(dataItem, slotLoadDatas);
                return true;
            }
        }

        List<InventorySlot> emptySlots = EmptySlots();
        if (emptySlots.Count <= 0) return false;

        for (int i = 0; i < emptySlots.Count; i++)
        {
            int loadAmount = Mathf.Min(maxAmount, loadCount);

            slotLoadDatas[emptySlots[i]] = loadAmount;
            loadCount -= loadAmount;

            if (loadCount <= 0)
            {
                Load_AllSlots(dataItem, slotLoadDatas);
                return true;
            }
        }
        return false;
    }
    private void Load_AllSlots(Item_ScrObj loadItem, Dictionary<InventorySlot, int> slotLoadDatas)
    {
        foreach (var data in slotLoadDatas)
        {
            InventorySlot slot = data.Key;
            int amount = data.Value;

            if (slot.data == null)
            {
                slot.Set_Data(new(loadItem, amount));
                continue;
            }

            ItemData slotData = slot.data;
            slotData.Update_CurrentAmount(slotData.amount + amount);
        }
    }

    public void Load_AllSlots()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            InventorySlot slot = _slots[i];

            if (slot == null)
            {
                Debug.Log("Inspector Slot Field Empty at Index : " + i);
                return;
            }

            slot.Update_ItemImage();
            slot.Update_AmountText();
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(Inventory_Manager))]
public class InventoryManager : Editor
{
    private SerializedProperty loadItemProp;
    private SerializedProperty loadItemAmountProp;

    private void OnEnable()
    {
        loadItemProp = serializedObject.FindProperty("loadItem");
        loadItemAmountProp = serializedObject.FindProperty("loadItemAmount");
    }

    public override void OnInspectorGUI()
    {
        Inventory_Manager manager = (Inventory_Manager)target;

        serializedObject.Update();
        DrawDefaultInspector();

        GUILayout.Space(40);
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(loadItemProp, GUIContent.none);
        Item_ScrObj loadItem = (Item_ScrObj)loadItemProp.objectReferenceValue;

        EditorGUILayout.PropertyField(loadItemAmountProp, GUIContent.none);
        int loadAmount = loadItemAmountProp.intValue;

        if (GUILayout.Button("Load Item"))
        {
            if (loadItem == null) return;

            manager.Load_ItemData(new(loadItem, loadAmount));
            manager.Load_AllSlots();
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif