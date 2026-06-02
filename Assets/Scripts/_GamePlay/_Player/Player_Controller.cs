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
    [SerializeField] private PlayerData _maxData;
    public PlayerData maxData => _maxData;


    private PlayerData _data;
    public PlayerData data => _data;

    public Action<int> OnHealthUpdate;
    public Action<int> OnHungerUpdate;
    public Action<int> OnTemperatureUpdate;
    public Action<int> OnStaminaUpdate;


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
        ES3.Save(SaveKeys.Player_SaveKeys.Data, _data ?? _maxData);
    }

    public void Load_Data()
    {
        _data = ES3.Load(SaveKeys.Player_SaveKeys.Data, new PlayerData(_maxData));

        if (ES3.KeyExists(SaveKeys.Player_SaveKeys.Data)) return;
        _data.Update_Stamina(_maxData.stamina);
    }


    // IDamageable
    public bool IsDamageable()
    {
        return _data.health > 0;
    }

    public int InflictDamage(int damageValue)
    {
        Update_Health(_data.health - damageValue);
        _animationPlayer.Play(1);

        return _data.health;
    }


    // Data
    public void Update_Health(int updateValue)
    {
        updateValue = Mathf.Min(updateValue, _maxData.health);
        if (updateValue == _data.health) return;

        OnHealthUpdate?.Invoke(_data.Update_Health(updateValue));

        if (_data.hunger <= _data.health) return;
        Update_Hunger(_data.health);
    }

    public void Update_Hunger(int updateValue)
    {
        updateValue = Mathf.Clamp(updateValue, 0, _data.health);
        if (updateValue == _data.hunger) return;

        OnHungerUpdate?.Invoke(_data.Update_Hunger(updateValue));
    }

    public void Update_Temperature(int updateValue)
    {
        updateValue = Mathf.Min(updateValue, _maxData.temperature);
        if (updateValue == _data.temperature) return;

        OnTemperatureUpdate?.Invoke(_data.Update_Temperature(updateValue));
    }

    public void Update_Stamina(int updateValue)
    {
        updateValue = Mathf.Min(updateValue, _maxData.stamina);
        if (updateValue == _data.stamina) return;
        
        OnStaminaUpdate?.Invoke(_data.Update_Stamina(updateValue));
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
        _movement.tileTracker.Load_CurrentTile(setTile);
    }

    private void Set_InventoryBagpack()
    {
        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        ItemData itemData = _inventoryBagpack != null ? new(_inventoryBagpack, 1) : null;

        itemCursor.Set_Data(itemData);
        itemCursor.Update_Visuals();
    }
}