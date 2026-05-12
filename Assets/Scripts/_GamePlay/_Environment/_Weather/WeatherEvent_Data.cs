using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeatherEvent_Data
{
    [SerializeField] private Weather_ScrObj _weather;
    public Weather_ScrObj weather => _weather;

    [SerializeField] private int _timeCount;
    public int timeCount => _timeCount;

    public WeatherEvent_Data(Weather_ScrObj trackWeather, int trackStartTime)
    {
        _weather = trackWeather;
        _timeCount = trackStartTime;
    }

    public void Update_TimeCount(int updateValue)
    {
        _timeCount = Mathf.Max(0, updateValue);
    }
}
