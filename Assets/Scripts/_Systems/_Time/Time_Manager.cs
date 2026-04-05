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

public class TimeCount_UpdateData
{
    private object _obj;
    public object obj => _obj;

    private int _updateValue;
    public int updateValue => _updateValue;

    public TimeCount_UpdateData(object obj, int value)
    {
        _obj = obj;
        _updateValue = value;
    }

    public void Set_UpdateValue(int setValue)
    {
        _updateValue = Mathf.Max(0, setValue);
    }
}

public class Time_Manager : MonoBehaviour, ISaveLoadable
{
    [Space(20)]
    [SerializeField][Range(0, 1000)] private int _maxTimeCount;
    [SerializeField][Range(0, 1000)] private int _nightPhaseCount;

    [Space(10)]
    [SerializeField][Range(0, 100)] private float _tikTime;


    private TimeData _data;
    public TimeData data => _data;

    private List<TimeCount_UpdateData> _countUpdateDatas = new();
    private Dictionary<TimeUpdateBus, Action> _timeUpdateBuses = new();

    public Action<int> OnTimeCount;
    public Action OnNightPhaseUpdate;

    public Action<int> OnDayCount;
    public Action OnDayUpdate;

    private Coroutine _timeTikCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Input_Controller input = Input_Controller.instance;
        input.OnInteract -= Toggle_TimeTik;

        InGame_Manager manager = InGame_Manager.instance;

        manager.player.movement.OnMovement -= Stop_TimTik;
        manager.tilesController.OnTileSelect -= Stop_TimTik;
        manager.cursor.OnTilePointRangeUpdate -= Stop_TimTik;
    }


    // ISaveLoadable
    public void Save_Data()
    {
        ES3.Save(SaveKeys.Time_SaveKeys.Data, _data ?? new TimeData(0, 0));
    }

    public void Load_Data()
    {
        _data = ES3.Load(SaveKeys.Time_SaveKeys.Data, new TimeData(0, 0));
    }


    // Data
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;
        input.OnInteract += Toggle_TimeTik;

        InGame_Manager manager = InGame_Manager.instance;

        manager.player.movement.OnMovement += Stop_TimTik;
        manager.tilesController.OnTileSelect += Stop_TimTik;
        manager.cursor.OnTilePointRangeUpdate += Stop_TimTik;
    }


    public bool Is_Night()
    {
        return _data.timeCount >= _nightPhaseCount;
    }

    private void Run_TimeUpdates()
    {
        for (int i = 0; i < _timeUpdateBuses.Count; i++)
        {
            TimeUpdateBus runBus = (TimeUpdateBus)i;
            _timeUpdateBuses[runBus]?.Invoke();
        }

        OnTimeCount?.Invoke(_data.timeCount);

        if (_data.timeCount != _nightPhaseCount) return;
        OnNightPhaseUpdate?.Invoke();
    }


    public int TimeCount_ValueUpdateSum()
    {
        int sum = 0;

        foreach (TimeCount_UpdateData data in _countUpdateDatas)
        {
            sum += data.updateValue;
        }
        return sum;
    }

    public void Track_TimeCountData(TimeCount_UpdateData dataToTrack)
    {
        for (int i = 0; i < _countUpdateDatas.Count; i++)
        {
            TimeCount_UpdateData data = _countUpdateDatas[i];
            if (dataToTrack.obj != data.obj) continue;

            data.Set_UpdateValue(dataToTrack.updateValue);
            return;
        }
        _countUpdateDatas.Add(dataToTrack);
    }
    public void Count_Time()
    {
        int calculatedTimeCount = data.timeCount + Mathf.Max(0, TimeCount_ValueUpdateSum());

        if (calculatedTimeCount <= _maxTimeCount)
        {
            _data.Set_Data(calculatedTimeCount, data.dayCount);
            Run_TimeUpdates();

            return;
        }

        int dayUpdateCount = Mathf.FloorToInt(calculatedTimeCount / _maxTimeCount);

        _data.Set_Data(calculatedTimeCount % _maxTimeCount - 1, _data.dayCount + dayUpdateCount);

        OnDayUpdate?.Invoke();
        OnDayCount?.Invoke(_data.dayCount);

        Run_TimeUpdates();
    }


    // Data Update
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


    // Time Tik Count
    private void Toggle_TimeTik(bool toggle)
    {
        if (_timeTikCoroutine != null)
        {
            StopCoroutine(_timeTikCoroutine);
            _timeTikCoroutine = null;
        }

        if (toggle == false) return;
        if (InGame_Manager.instance.movements.AllMovements_Complete() == false) return;

        _timeTikCoroutine = StartCoroutine(Run_TimeTik());
    }
    private IEnumerator Run_TimeTik()
    {
        float restrictedTikTime = Mathf.Max(0.1f, _tikTime);

        while (true)
        {
            yield return new WaitForSeconds(restrictedTikTime);
            Count_Time();
        }
    }

    private void Toggle_TimeTik()
    {
        Toggle_TimeTik(_timeTikCoroutine == null);
    }
    private void Stop_TimTik()
    {
        if (_timeTikCoroutine == null) return;
        Toggle_TimeTik(false);
    }
}