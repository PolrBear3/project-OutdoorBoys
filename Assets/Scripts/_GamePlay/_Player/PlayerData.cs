using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [ES3Serializable][SerializeField][Range(0, 500)] private int _health;
    public int health => _health;

    [ES3Serializable][SerializeField][Range(0, 500)] private int _hunger;
    public int hunger => _hunger;

    [ES3Serializable][SerializeField][Range(0, 500)] private int _temperature;
    public int temperature => _temperature;

    [ES3Serializable][SerializeField][Range(0, 500)] private int _stamina;
    public int stamina => _stamina;

    /// <summary>
    /// ItemData (item & time count duration), float cooltime decrease value
    /// </summary>
    [ES3Serializable] private Dictionary<ItemData, float> _coolTimeDecreaseDatas = new();
    public Dictionary<ItemData, float> coolTimeDecreaseDatas => _coolTimeDecreaseDatas;


    // New Constructors
    public PlayerData(PlayerData copyData)
    {
        _health = copyData._health;
        _hunger = copyData._hunger;
        _temperature = copyData._temperature;
        _stamina = copyData._stamina;
    }


    // Datas
    public int Update_Health(int updateValue)
    {
        _health = Mathf.Max(0, updateValue);
        return _health;
    }

    public int Update_Hunger(int updateValue)
    {
        _hunger = Mathf.Clamp(updateValue, 0, _health);
        return _hunger;
    }

    public int Update_Temperature(int updateValue)
    {
        _temperature = Mathf.Max(0, updateValue);
        return _temperature;
    }

    public int Update_Stamina(int updateValue)
    {
        _stamina = Mathf.Max(0, updateValue);
        return _stamina;
    }


    // Cool Time Decrease
    public float Total_CoolTimeDecreaseValue()
    {
        float totalValue = 0;

        foreach (var data in _coolTimeDecreaseDatas)
        {
            totalValue += data.Value;
        }
        return totalValue;
    }

    public void Update_CoolTimeDecrease(ItemData itemDurationData, float buffValue)
    {
        foreach (var data in _coolTimeDecreaseDatas)
        {
            if (data.Key.itemScrObj != itemDurationData.itemScrObj) continue;

            _coolTimeDecreaseDatas.Remove(data.Key);
            break;
        }
        _coolTimeDecreaseDatas[itemDurationData] = buffValue;
    }
}