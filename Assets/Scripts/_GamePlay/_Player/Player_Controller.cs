using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Controller : MonoBehaviour, ISaveLoadable, IDamageable
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animationPlayer;
    public AnimationPlayer animationPlayer => _animationPlayer;

    [SerializeField] private Player_Movement _movement;
    public Player_Movement movement => _movement;

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
    public Action<int> OnTemperatureUpdate;
    public Action<int, int> OnStaminaUpdate;


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

        if (ES3.KeyExists(SaveKeys.Player_SaveKeys.Data)) return;
        _data.Update_CurrentStamina(_data.maxStamina);
    }


    // IDamageable
    public int InflictDamage(int damageValue)
    {
        Update_Health(_data.health - damageValue);
        _animationPlayer.Play(2);

        return _data.health;
    }


    // Data
    public void Update_Health(int updateValue)
    {
        OnHealthUpdate?.Invoke(_data.Update_Health(updateValue));
    }

    public void Update_Temperature(int updateValue)
    {
        OnTemperatureUpdate?.Invoke(_data.Update_Temperature(updateValue));
    }

    public void Update_MaxStamina(int updateValue)
    {
        OnStaminaUpdate?.Invoke(_data.Update_MaxStamina(updateValue), _data.currentStamina);
    }
    public void Update_CurrentStamina(int updateValue)
    {
        OnStaminaUpdate?.Invoke(_data.maxStamina, _data.Update_CurrentStamina(updateValue));

        if (updateValue >= 0) return;
        Update_Health(_data.health + updateValue);
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

        transform.position = setTile.Random_BoundPoint();
        _movement.tileTracker.Set_Data(setTile);
    }

    private void Set_InventoryBagpack()
    {
        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        ItemData itemData = _inventoryBagpack != null ? new(_inventoryBagpack, 1) : null;

        itemCursor.Set_Data(itemData);
        itemCursor.Update_Visuals();
    }
}