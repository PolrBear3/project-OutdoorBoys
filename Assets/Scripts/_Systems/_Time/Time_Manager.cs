using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TimeUpdateBus
{
    AwakeUpdate = 0,
    StartUpdate = 1,
    SubUpdate = 2
}

public class Time_Manager : MonoBehaviour, ISaveLoadable
{
    [Space(20)]
    [SerializeField][Range(0, 1000)] private int _maxTimeCount;
    [SerializeField][Range(0, 1000)] private int _nightPhaseTime;

    [Space(10)]
    [SerializeField][Range(0, 100)] private int _rewardTargetUpdateTime;


    private TimeData _data;
    public TimeData data => _data;

    private Dictionary<TimeUpdateBus, Action> _timeUpdateBuses = new();

    private HashSet<object> _timeUpdateActions = new();
    public HashSet<object> timeUpdateActions => _timeUpdateActions;

    public Action OnNightPhaseTime;
    public Action<int> OnDayCount;
    public Action OnRewardTargetTime;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Input_Controller.instance.OnHoldInteract -= Count_Time;
    }


    // ISaveLoadable
    public void Save_Data()
    {
        ES3.Save(SaveKeys.Time_SaveKeys.Data, _data ?? new TimeData(0, 0, _rewardTargetUpdateTime));
    }

    public void Load_Data()
    {
        _data = ES3.Load(SaveKeys.Time_SaveKeys.Data, new TimeData(0, 0, _rewardTargetUpdateTime));
    }


    // Data
    private void Set_Data()
    {
        Input_Controller.instance.OnHoldInteract += Count_Time;
    }

    public bool Is_Night()
    {
        return _data.timeCount >= _nightPhaseTime;
    }

    public bool TimeUpdateActions_Running()
    {
        return _timeUpdateActions.Count > 0;
    }


    private void Run_TimeUpdates()
    {
        for (int i = 0; i < _timeUpdateBuses.Count; i++)
        {
            TimeUpdateBus runBus = (TimeUpdateBus)i;

            if (_timeUpdateBuses.TryGetValue(runBus, out Action action) == false) continue;
            action?.Invoke();
        }

        if (_data.timeCount != _nightPhaseTime) return;
        OnNightPhaseTime?.Invoke();
    }

    public void Run_RewardUpdates(bool run)
    {
        if (run == false) return;
        
        OnRewardTargetTime?.Invoke();
    }

    public void Count_Time()
    {
        int calculatedTimeCount = _data.timeCount + 1;

        if (calculatedTimeCount <= _maxTimeCount)
        {
            int currentRewardTime = _data.rewardTargetTime;
            int calculatedTargetTime = currentRewardTime + _rewardTargetUpdateTime;

            bool activateReward = calculatedTimeCount == currentRewardTime;
            int rewardUpdateTime = calculatedTargetTime <= _maxTimeCount ? calculatedTargetTime : _rewardTargetUpdateTime;

            _data.Update_RewardTargetTime(activateReward ? rewardUpdateTime : currentRewardTime);
            _data.Set_Data(calculatedTimeCount, data.dayCount);

            Run_RewardUpdates(activateReward);
            Run_TimeUpdates();

            return;
        }

        int dayUpdateCount = Mathf.FloorToInt(calculatedTimeCount / _maxTimeCount);

        _data.Set_Data(0, _data.dayCount + dayUpdateCount);
        _data.Update_RewardTargetTime(_rewardTargetUpdateTime);

        OnDayCount?.Invoke(_data.dayCount);
        Run_TimeUpdates();
    }


    // Time Update Bus
    public void Register(TimeUpdateBus updateBus, Action targetAction)
    {
        if (_timeUpdateBuses.ContainsKey(updateBus) == false)
        {
            _timeUpdateBuses.Add(updateBus, targetAction);
            return;
        }
        _timeUpdateBuses[updateBus] += targetAction;
    }

    public void UnRegister(TimeUpdateBus updateBus, Action targetAction)
    {
        _timeUpdateBuses[updateBus] -= targetAction;
    }
}