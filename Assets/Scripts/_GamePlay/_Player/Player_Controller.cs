using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animationPlayer;
    public AnimationPlayer animationPlayer => _animationPlayer;

    [SerializeField] private Player_Movement _movement;
    public Player_Movement movement => _movement;

    [SerializeField] private Player_Interaction _interaction;
    public Player_Interaction interaction => _interaction;

    [Space(20)]
    [SerializeField] private Item_ScrObj _inventoryBagpackScrObj;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Set_Position);
        EventBus_Manager.Register(EventBus.StartLoad, Set_InventoryBagpack);
    }

    void Start()
    {
        _animationPlayer.Play(0);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_Position);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_InventoryBagpack);
    }


    // Game Load
    private void Set_Position()
    {
        // Tile setTile = InGame_Manager.instance.tilesController.Current_Tile(TileType.softGround);
        
        Tile setTile = InGame_Manager.instance.tilesController.currentTiles[1];
        if (setTile == null) return;
        
        _movement.MoveTo_Tile(setTile);
    }

    private void Set_InventoryBagpack()
    {
        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        
        if (_inventoryBagpackScrObj == null)
        {
            itemCursor.Set_Item(null);
            return;
        }
        itemCursor.Set_Item(new(_inventoryBagpackScrObj, 1));
    }
}