using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemSynergy_EffectType
{
    playerDataUpdate,
    playerTemperatureSustain,
    tileStateUpdate,
    placeItem,
    replaceItem
}

public interface IItemSynergy_EffectRunner
{
    ItemSynergy_EffectType effectType { get; }

    void Run_Effect(Tile targetTile, ItemSynergy_EffectData effectData);
}

[System.Serializable]
public class ItemSynergy_EffectData
{
    [SerializeField] private ItemSynergy_EffectType _effectType;
    public ItemSynergy_EffectType effectType => _effectType;

    [Space(20)]
    [SerializeField] private PlayerData_ModifierData _playerModifyData;
    public PlayerData_ModifierData playerModifyData => _playerModifyData;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _temperatureSustainValue;
    public int temperatureSustainValue => _temperatureSustainValue;

    [SerializeField][Range(0, 100)] private int _damageValue;
    public int damageValue => _damageValue;

    [Space(20)]
    [SerializeField] private TileState_Data[] _stateUpdateDatas;
    public TileState_Data[] stateUpdateDatas => _stateUpdateDatas;

    [Space(20)]
    [SerializeField] private ItemData[] _placeItems;
    public ItemData[] placeItems => _placeItems;

    [SerializeField] private ItemData[] _replaceItems;
    public ItemData[] replaceItems => _replaceItems;
}

public class PlayerDataUpdate_EffectRunner : IItemSynergy_EffectRunner
{
    public ItemSynergy_EffectType effectType => ItemSynergy_EffectType.playerDataUpdate;

    public void Run_Effect(Tile targetTile, ItemSynergy_EffectData effectData)
    {
        Player_Controller player = InGame_Manager.instance.player;
        PlayerData data = player.data;

        PlayerData_ModifierData updateData = effectData.playerModifyData;

        player.Update_Health(data.health + updateData.healthUpdateValue);
        player.Update_Hunger(data.hunger + updateData.hungerUpdateValue);
        player.Update_Temperature(data.temperature + updateData.temperatureUpdateValue);
        player.Update_Stamina(data.stamina + updateData.staminaUpdateValue);
    }
}

public class PlayerTemperatureSustain_EffectRunner : IItemSynergy_EffectRunner
{
    public ItemSynergy_EffectType effectType => ItemSynergy_EffectType.playerTemperatureSustain;

    public void Run_Effect(Tile targetTile, ItemSynergy_EffectData effectData)
    {

    }
}

public class TileStateUpdate_EffectRunner : IItemSynergy_EffectRunner
{
    public ItemSynergy_EffectType effectType => ItemSynergy_EffectType.tileStateUpdate;

    public void Run_Effect(Tile targetTile, ItemSynergy_EffectData effectData)
    {

    }
}

public class PlaceItem_EffectRunner : IItemSynergy_EffectRunner
{
    public ItemSynergy_EffectType effectType => ItemSynergy_EffectType.placeItem;

    public void Run_Effect(Tile targetTile, ItemSynergy_EffectData effectData)
    {

    }
}

public class ReplaceItem_EffectRunner : IItemSynergy_EffectRunner
{
    public ItemSynergy_EffectType effectType => ItemSynergy_EffectType.replaceItem;

    public void Run_Effect(Tile targetTile, ItemSynergy_EffectData effectData)
    {

    }
}