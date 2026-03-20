using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventBus
{
    AwakeLoad = 0,
    StartLoad = 1,
    SubLoad = 2
}

public static class EventBus_Manager
{
    private static readonly Dictionary<EventBus, Action> _eventBuses = new();


    // Register
    public static void Register(EventBus eventState, Action targetAction)
    {
        if (_eventBuses.ContainsKey(eventState) == false)
        {
            _eventBuses.Add(eventState, targetAction);
            return;
        }
        _eventBuses[eventState] += targetAction;
    }

    public static void UnRegister(EventBus eventState, Action targetAction)
    {
        _eventBuses[eventState] -= targetAction;
    }


    // Run
    public static void Run_BusEvents()
    {
        if (_eventBuses.Count <= 0)
        {
            Debug.Log("Event Bus Empty!");
        }
        
        for (int i = 0; i < _eventBuses.Count; i++)
        {
            EventBus runBus = (EventBus)i;
            _eventBuses[runBus]?.Invoke();
        }
    }
}