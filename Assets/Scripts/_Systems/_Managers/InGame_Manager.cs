using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGame_Manager : MonoBehaviour
{
    public static InGame_Manager instance;


    [Space(20)]
    [SerializeField] private InGameUI_Manager _ingameUI;
    public InGameUI_Manager ingameUI => _ingameUI;

    [SerializeField] private MovementControllers_Manager _movements;
    public MovementControllers_Manager movements => _movements;


    [Space(20)]
    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;

    [SerializeField] private Time_Manager _time;
    public Time_Manager time => _time;

    [SerializeField] private Inventory_Manager _inventory;
    public Inventory_Manager inventory => _inventory;

    [SerializeField] private ItemCrafting_Manager _itemCrafting;
    public ItemCrafting_Manager itemCrafting => _itemCrafting;

    [SerializeField] private Animals_Manager _animals;
    public Animals_Manager animals => _animals;


    [Space(20)]
    [SerializeField] private WorldMap_Generator _worldMapGenerator;
    public WorldMap_Generator worldMapGenerator => _worldMapGenerator;

    [SerializeField] private Tiles_Controller _tilesController;
    public Tiles_Controller tilesController => _tilesController;


    [Space(20)]
    [SerializeField] private Player_Controller _player;
    public Player_Controller player => _player;


    // MonoBehaviour
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EventBus_Manager.Run_BusEvents();
    }


    // Data
    public List<ItemData> Current_ItemDatas()
    {
        List<ItemData> allDatas = new();

        allDatas.AddRange(_inventory.slotManager.Current_ItemDatas());
        allDatas.AddRange(_tilesController.Placed_ItemDatas());
        allDatas.Add(_cursor.itemCursor.data);

        List<ItemData> combinedDatas = new();

        for (int i = 0; i < allDatas.Count; i++)
        {
            ItemData itemData = allDatas[i];
            Item_ScrObj item = itemData.itemScrObj;

            int amount = itemData.amount;
            bool amountUpdated = false;

            for (int j = 0; j < combinedDatas.Count; j++)
            {
                ItemData combinedData = combinedDatas[j];

                if (item != combinedData.itemScrObj) continue;
                combinedData.Update_CurrentAmount(combinedData.amount + amount);

                amountUpdated = true;
                break;
            }

            if (amountUpdated) continue;
            combinedDatas.Add(new(item, amount));
        }
        return allDatas;
    }
}
