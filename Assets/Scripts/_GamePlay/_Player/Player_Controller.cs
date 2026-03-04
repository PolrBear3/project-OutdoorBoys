using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour, ISaveLoadable
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animationPlayer;
    public AnimationPlayer animationPlayer => _animationPlayer;

    [SerializeField] private Movement_Controller _movement;
    public Movement_Controller movement => _movement;

    [SerializeField] private Player_Interaction _interaction;
    public Player_Interaction interaction => _interaction;

    [Space(20)]
    [SerializeField] private Item_ScrObj _inventoryBagpack;
    public Item_ScrObj inventoryBagpack => _inventoryBagpack;


    [Space(20)]
    [SerializeField] private PlayerData _defaultData;

    private PlayerData _data;
    public PlayerData data => _data;


    public Action<int> OnHealthUpdate;
    public Action<int> OnHungerUpdate;
    public Action<int> OnTemperatureUpdate;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Set_Position);
        EventBus_Manager.Register(EventBus.StartLoad, Set_Animation);
        EventBus_Manager.Register(EventBus.StartLoad, Set_InventoryBagpack);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_Position);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_Animation);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_InventoryBagpack);
    }


    // ISaveLoadable
    public void Save_Data()
    {
        ES3.Save(SaveKeys.Player_SaveKeys.Data, _data ?? _defaultData);
    }

    public void Load_Data()
    {
        _data = ES3.Load(SaveKeys.Player_SaveKeys.Data, _defaultData);
    }


    // Data
    public void Update_Health(int updateValue)
    { 
        OnHealthUpdate?.Invoke(_data.Update_Health(updateValue));
    }

    public void Update_Hunger(int updateValue)
    {
        OnHungerUpdate?.Invoke(_data.Update_Hunger(updateValue));
    }

    public void Update_Temperature(int updateValue)
    {
        OnTemperatureUpdate?.Invoke(_data.Update_Temperature(updateValue));
    }


    // Game Load
    private void Set_Animation()
    {
        _animationPlayer.Play(0);
    }

    private void Set_Position()
    {
        Tile setTile = InGame_Manager.instance.tilesController.Current_Tile(TileType.softGround);
        if (setTile == null) return;
        
        _movement.MoveTo_Tile(setTile);
    }

    private void Set_InventoryBagpack()
    {
        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        
        ItemData itemData = _inventoryBagpack != null ? new(_inventoryBagpack, 1) : null;
        itemCursor.Set_Data(itemData);
        
        itemCursor.Update_Visuals();
    }
}