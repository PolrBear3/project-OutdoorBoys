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
    [SerializeField][Range(0, 1000)] private int _nightPhaseCount;

    [Space(10)]
    [SerializeField][Range(0, 100)] private float _tikTime;


    private TimeData _data;
    public TimeData data => _data;

    private readonly Dictionary<TimeUpdateBus, Action> _timeUpdateBuses = new();

    public Action<int> OnTimeCountUpdate;
    public Action OnNightPhaseUpdate;

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


    // Register
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


    public void Run_TimeUpdate()
    {
        for (int i = 0; i < _timeUpdateBuses.Count; i++)
        {
            TimeUpdateBus runBus = (TimeUpdateBus)i;
            _timeUpdateBuses[runBus]?.Invoke();
        }
        OnTimeCountUpdate?.Invoke(_data.timeCount);

        if (_data.timeCount != _nightPhaseCount) return;
        OnNightPhaseUpdate?.Invoke();
    }

    public void Update_Data(int updateTimeCount)
    {
        int calculatedTimeCount = data.timeCount + Mathf.Max(0, updateTimeCount);

        if (calculatedTimeCount <= _maxTimeCount)
        {
            _data.Set_Data(calculatedTimeCount, data.dayCount);
            Run_TimeUpdate();

            return;
        }

        int dayUpdateCount = Mathf.FloorToInt(calculatedTimeCount / _maxTimeCount);

        _data.Set_Data(calculatedTimeCount % _maxTimeCount - 1, _data.dayCount + dayUpdateCount);
        Run_TimeUpdate();
    }

    public bool Is_Night()
    {
        return _data.timeCount >= _nightPhaseCount;
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
        _timeTikCoroutine = StartCoroutine(Run_TimeTik());
    }
    private IEnumerator Run_TimeTik()
    {
        float restrictedTikTime = Mathf.Max(0.1f, _tikTime);

        while (true)
        {
            yield return new WaitForSeconds(restrictedTikTime);
            Update_Data(1);
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
