using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeRange_Data
{
    [SerializeField][Range(0, 100)] private int _activeDay;
    public int activeDay => _activeDay;

    [Space(10)]
    [SerializeField][Range(0, 100)] private int _activeTime;
    [SerializeField][Range(0, 100)] private int _restrictTime;

    [Space(10)]
    [SerializeField][Range(0, 100)] private int _minTimeCount;
    [SerializeField][Range(0, 100)] private int _maxTimeCount;


    // Converted Data
    public bool Is_ActiveDay()
    {
        return InGame_Manager.instance.time.data.dayCount >= _activeDay;
    }

    public int ActiveTime()
    {
        return Mathf.Min(_activeTime, InGame_Manager.instance.time.maxTimecount);
    }
    public bool Is_ActiveTime()
    {
        return InGame_Manager.instance.time.data.timeCount >= ActiveTime();
    }

    public int RestrictTime()
    {
        int maxtimeCount = InGame_Manager.instance.time.maxTimecount;

        return _restrictTime > 0 ? Mathf.Min(_restrictTime, maxtimeCount) : maxtimeCount + 1;
    }
    public bool Is_RestrictTime()
    {
        return InGame_Manager.instance.time.data.timeCount >= RestrictTime();
    }

    public int Random_TimeCount()
    {
        return Mathf.Max(1, Random.Range(_minTimeCount, _maxTimeCount + 1));
    }
}