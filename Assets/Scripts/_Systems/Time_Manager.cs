using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Time_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField][Range(0, 100)] private int _nightTimePoint;
    [SerializeField][Range(0, 100)] private int _dayTimeCount;

    [Space(10)]
    [SerializeField][Range(0, 100)] private float _tikTime;
    [SerializeField][Range(0, 100)] private float _boostTikTime;


    private TimeData _data;
    public TimeData data => _data;

    private bool _boostToggled;
    private Coroutine _timeTikCoroutine;

    public Action<bool> OnNightTime;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.Register(EventBus.AwakeLoad, Toggle_TimeTik);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Toggle_TimeTik);
    }


    // Data
    public void Set_Data()
    {
        _data = new(_dayTimeCount);
    }


    // Tik Update
    public void Toggle_BoostTimeTik(bool toggle)
    {
        _boostToggled = toggle;

        Toggle_TimeTik(_timeTikCoroutine != null);
    }

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
        float nightTimePoint = Mathf.Clamp(_nightTimePoint, 0, _dayTimeCount);

        while (true)
        {
            float tikTime = _boostToggled ? _boostTikTime : _tikTime;
            yield return new WaitForSeconds(tikTime);

            _data.UpdateData(1);
            int currentTimeCount = _data.timeCount;

            if (currentTimeCount <= 0)
            {
                OnNightTime?.Invoke(false);
                continue;
            }

            if (currentTimeCount != _nightTimePoint) continue;
            OnNightTime?.Invoke(true);
        }
    }


    private void Toggle_TimeTik()
    {
        Toggle_TimeTik(true);
    }
}
