using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [ES3Serializable][SerializeField][Range(0, 500)] private int _health;
    public int health => _health;

    [ES3Serializable][SerializeField][Range(0, 500)] private int _temperature;
    public int temperature => _temperature;

    [ES3Serializable][SerializeField][Range(0, 500)] private int _stamina;
    public int stamina => _stamina;

    [ES3Serializable] private int _currentStamina;
    public int currentStamina => _currentStamina;


    // Data
    public int Update_Health(int updateValue)
    {
        _health = Mathf.Max(0, updateValue);
        return _health;
    }

    public int Update_Temperature(int updateValue)
    {
        _temperature = Mathf.Max(0, updateValue);
        return _temperature;
    }

    public int Update_Stamina(int updateValue)
    {
        _currentStamina = Mathf.Clamp(updateValue, 0, _stamina);
        return _currentStamina;
    }
}