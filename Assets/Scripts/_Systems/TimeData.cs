using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeData
{
    private int _maxTimeCount;

    private int _dayCount;
    public int dayCount => _dayCount;

    private int _timeCount;
    public int timeCount => _timeCount;


    public Action OnTimeCountUpdate;
    public Action OnDayCountUpdate;


    // Constructors
    public TimeData(int maxTimeCount)
    {
        _maxTimeCount = maxTimeCount;
    }


    // Data
    public void UpdateData(int updateTimeCount)
    {
        OnTimeCountUpdate?.Invoke();

        if (_timeCount + updateTimeCount <= _maxTimeCount)
        {
            _timeCount += updateTimeCount;
            return;
        }

        _timeCount = 0;
        _dayCount++;

        OnDayCountUpdate?.Invoke();
    }
}