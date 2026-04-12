using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeData
{
    private int _timeCountValue;
    public int timeCountValue => _timeCountValue;
    
    [ES3Serializable] private int _timeCount;
    public int timeCount => _timeCount;

    [ES3Serializable] private int _dayCount;
    public int dayCount => _dayCount;


    // Constructors
    public TimeData(int timeCount, int dayCount)
    {
        _timeCount = timeCount;
        _dayCount = dayCount;
    }


    // Data
    public void Set_Data(int timeCount, int dayCount)
    {
        _timeCount = timeCount;
        _dayCount = dayCount;
    }

    public void Update_TimeCountValue(int updateValue)
    {
        _timeCountValue = Mathf.Max(1, updateValue);
    }
}