using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeData
{
    [SerializeField] private int _timeCount;
    public int timeCount => _timeCount;

    [SerializeField] private int _dayCount;
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
}