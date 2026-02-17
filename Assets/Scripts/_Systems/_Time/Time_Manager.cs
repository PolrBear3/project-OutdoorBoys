using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Time_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField][Range(0, 100)] private int _nightPhaseCount;
    [SerializeField][Range(0, 100)] private int _maxTimeCount;

    [Space(10)]
    [SerializeField][Range(0, 100)] private float _tikTime;


    private TimeData _data;
    public TimeData data => _data;

    public Action OnTimeCountUpdate;
    public Action OnNightPhase;
    public Action OnDayCountUpdate;
    
    private Coroutine _timeTikCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
    }


    // Data
    public void Set_Data()
    {
        _data = new(0, 0);
    }

    public void Update_Data(int updateTimeCount)
    {
        int calculatedTimeCount = data.timeCount + Mathf.Max(1, updateTimeCount);

        if (calculatedTimeCount <= _maxTimeCount)
        {
            data.Set_Data(calculatedTimeCount, data.dayCount);
            OnTimeCountUpdate?.Invoke();

            if (_data.timeCount != _nightPhaseCount) return;
            OnNightPhase?.Invoke();

            return;
        }

        int dayUpdateCount = Mathf.FloorToInt(calculatedTimeCount / _maxTimeCount);

        data.Set_Data(calculatedTimeCount % _maxTimeCount - 1, _data.dayCount + dayUpdateCount);
        OnTimeCountUpdate?.Invoke();

        for (int i = 0; i < dayUpdateCount; i++)
        {
            OnDayCountUpdate?.Invoke();
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
