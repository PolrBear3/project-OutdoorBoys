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
