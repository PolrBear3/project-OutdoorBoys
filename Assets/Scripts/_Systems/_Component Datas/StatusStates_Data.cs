using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusState
{
    stunned,
    injured
}

public class StatusStates_Data
{
    // current state, count
    private Dictionary<StatusState, int> _datas;
    public Dictionary<StatusState, int> datas => _datas;


    public StatusStates_Data()
    {
        _datas = new();
    }

    public void Register_State(StatusState registerState, int setCount)
    {
        if (_datas.ContainsKey(registerState))
        {
            _datas[registerState] += setCount;
            return;
        }
        _datas[registerState] = setCount;
    }

    public bool Update_StateCount(StatusState updateState)
    {
        if (_datas.ContainsKey(updateState) == false) return false;
        _datas[updateState]--;

        if (_datas[updateState] > 0) return true;

        _datas.Remove(updateState);
        return true;
    }
}