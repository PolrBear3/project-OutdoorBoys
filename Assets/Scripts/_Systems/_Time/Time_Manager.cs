using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Time_Manager : MonoBehaviour, ISaveLoadable
{
    [Space(20)]
    [SerializeField][Range(0, 1000)] private int _maxTimeCount;
    public int maxTimecount => _maxTimeCount;

    [Space(10)]
    [SerializeField][Range(0, 1000)] private int _nightActiveTime;
    [SerializeField][Range(0, 100)] private int _rewardTargetTime;


    private TimeData _data;
    public TimeData data => _data;

    private Dictionary<ActionUpdateBus, Action> _timeUpdateBuses = new();

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
        ES3.Save(SaveKeys.Time_SaveKeys.Data, _data ?? new TimeData(0, 0, _rewardTargetTime));
    }

    public void Load_Data()
    {
        _data = ES3.Load(SaveKeys.Time_SaveKeys.Data, new TimeData(0, 0, _rewardTargetTime));
    }


    // Time Update Bus
    public void Register(ActionUpdateBus updateBus, Action targetAction)
    {
        if (_timeUpdateBuses.ContainsKey(updateBus) == false)
        {
            _timeUpdateBuses.Add(updateBus, targetAction);
            return;
        }
        _timeUpdateBuses[updateBus] += targetAction;
    }
    public void UnRegister(ActionUpdateBus updateBus, Action targetAction)
    {
        _timeUpdateBuses[updateBus] -= targetAction;
    }

    private void Run_TimeUpdates()
    {
        for (int i = 0; i < _timeUpdateBuses.Count; i++)
        {
            ActionUpdateBus runBus = (ActionUpdateBus)i;

            if (_timeUpdateBuses.TryGetValue(runBus, out Action action) == false) continue;
            action?.Invoke();
        }

        if (_data.timeCount != _nightActiveTime) return;
        OnNightPhaseTime?.Invoke();
    }


    // Data
    private void Set_Data()
    {
        Input_Controller.instance.OnHoldInteract += Count_Time;
    }


    public bool Is_Night()
    {
        return _data.timeCount >= _nightActiveTime;
    }

    public int Total_NightTimeCount()
    {
        return _maxTimeCount - _nightActiveTime + 1;
    }
    public int Current_NightTimeCount()
    {
        if (_data.timeCount < _nightActiveTime) return 0;
        return _data.timeCount - _nightActiveTime + 1;
    }

    public bool TimeUpdateActions_Running()
    {
        return _timeUpdateActions.Count > 0;
    }


    public void Run_RewardUpdates(bool run)
    {
        if (run == false) return;

        OnRewardTargetTime?.Invoke();
    }

    public void Count_Time()
    {
        if (TimeUpdateActions_Running()) return;
        if (InGame_Manager.instance.movements.AllMovements_Complete() == false) return;

        int calculatedTimeCount = _data.timeCount + 1;

        if (calculatedTimeCount <= _maxTimeCount)
        {
            int currentRewardTime = _data.rewardTargetTime;
            int calculatedTargetTime = currentRewardTime + _rewardTargetTime;

            bool activateReward = calculatedTimeCount == currentRewardTime;
            int rewardUpdateTime = calculatedTargetTime <= _maxTimeCount ? calculatedTargetTime : _rewardTargetTime;

            _data.Update_RewardTargetTime(activateReward ? rewardUpdateTime : currentRewardTime);
            _data.Set_Data(calculatedTimeCount, data.dayCount);

            Run_RewardUpdates(activateReward);
            Run_TimeUpdates();

            return;
        }

        int dayUpdateCount = Mathf.FloorToInt(calculatedTimeCount / _maxTimeCount);

        _data.Set_Data(0, _data.dayCount + dayUpdateCount);
        _data.Update_RewardTargetTime(_rewardTargetTime);

        OnDayCount?.Invoke(_data.dayCount);
        Run_TimeUpdates();
    }
}