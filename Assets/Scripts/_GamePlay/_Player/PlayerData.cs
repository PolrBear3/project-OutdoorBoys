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


    // New Constructors
    public PlayerData(PlayerData copyData)
    {
        _health = copyData._health;
        _hunger = copyData._hunger;
        _temperature = copyData._temperature;
        _stamina = copyData._stamina;
    }


    // Data
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
}