using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimalAction : MonoBehaviour
{
    [SerializeField] private Animal _controller;
    public Animal controller => _controller;

    private bool _actionRunning;
    public bool actionRunning => _actionRunning;


    public virtual bool RunAction()
    {
        _actionRunning = false;
        return true;
    }

    public void Toggle_ActionRunningSignal(bool toggle)
    {
        _actionRunning = toggle;
    }
}