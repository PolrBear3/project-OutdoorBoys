using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [SerializeField][Range(0, 10)] private int _health;
    public int health => _health;

    [SerializeField][Range(0, 10)] private int _hunger;
    public int hunger => _hunger;

    [SerializeField][Range(0, 10)] private int _temperature;
    public int temperature => _temperature;

    [SerializeField][Range(0, 500)] private int _maxItemCarryWeight;
    public int maxItemCarryWeight => _maxItemCarryWeight;


    // Data
    public int Update_Health(int updateValue)
    {
        _health = Mathf.Max(0, updateValue);
        return _health;
    }

    public int Update_Hunger(int updateValue)
    {
        _hunger = Mathf.Max(0, updateValue);
        return _hunger;
    }

    public int Update_Temperature(int updateValue)
    {
        _temperature = Mathf.Max(0, updateValue);
        return _temperature;
    }

    public int Update_MaxItemCarryWeight(int updateValue)
    {
        _maxItemCarryWeight = Mathf.Max(0, updateValue);
        return _maxItemCarryWeight;
    }
}