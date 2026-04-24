using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeData
{
    [ES3Serializable] private int _timeCount;
    public int timeCount => _timeCount;

    [ES3Serializable] private int _dayCount;
    public int dayCount => _dayCount;

    [ES3Serializable] private int _rewardTargetTime;
    public int rewardTargetTime => _rewardTargetTime;


    // Constructors
    public TimeData(int timeCount, int dayCount, int rewardTargetTime)
    {
        _timeCount = timeCount;
        _dayCount = dayCount;
        _rewardTargetTime = rewardTargetTime;
    }


    // Data
    public void Set_Data(int timeCount, int dayCount)
    {
        _timeCount = timeCount;
        _dayCount = dayCount;
    }

    public void Update_RewardTargetTime(int updateValue)
    {
        _rewardTargetTime = Mathf.Max(1, updateValue);
    }
}