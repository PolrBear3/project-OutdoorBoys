using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Time_Manager : MonoBehaviour, ISaveLoadable
{
    [Space(20)]
    [SerializeField][Range(0, 1000)] private int _nightPhaseCount;
    [SerializeField][Range(0, 1000)] private int _maxTimeCount;

    [Space(10)]
    [SerializeField][Range(0, 100)] private float _tikTime;


    private TimeData _data;
    public TimeData data => _data;

    public Action<int> OnTimeCountUpdate;
    public Action OnNightPhase;
    public Action<int> OnDayCountUpdate;
    
    private Coroutine _timeTikCoroutine;


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
    public void Update_Data(int updateTimeCount)
    {
        int calculatedTimeCount = data.timeCount + Mathf.Max(1, updateTimeCount);

        if (calculatedTimeCount <= _maxTimeCount)
        {
            _data.Set_Data(calculatedTimeCount, data.dayCount);
            OnTimeCountUpdate?.Invoke(_data.timeCount);

            if (_data.timeCount != _nightPhaseCount) return;
            OnNightPhase?.Invoke();

            return;
        }

        int dayUpdateCount = Mathf.FloorToInt(calculatedTimeCount / _maxTimeCount);

        _data.Set_Data(calculatedTimeCount % _maxTimeCount - 1, _data.dayCount + dayUpdateCount);
        OnTimeCountUpdate?.Invoke(_data.timeCount);

        for (int i = 0; i < dayUpdateCount; i++)
        {
            OnDayCountUpdate?.Invoke(_data.dayCount);
            OnNightPhase?.Invoke();
        }

        if (_data.timeCount < _nightPhaseCount) return;
        OnNightPhase?.Invoke();
    }


    public bool Is_Night()
    {
        return _data.timeCount >= _nightPhaseCount;
    }


    // Time Tik Count
    public void Toggle_TimeTik(bool toggle)
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
        float restrictedTikTime = Mathf.Min(0.1f, _tikTime);
        
        while (true)
        {
            yield return new WaitForSeconds(restrictedTikTime);
            Update_Data(1);
        }
    }
}
