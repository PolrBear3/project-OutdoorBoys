using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimalAction : MonoBehaviour
{
    [SerializeField] private Animal _controller;
    public Animal controller => _controller;


    private bool _actionRunning;
    public bool actionRunning => _actionRunning;


    // Main
    public virtual void Run_Action()
    {
        _actionRunning = false;
    }

    public void Toggle_ActionRunningSignal(bool toggle)
    {
        _actionRunning = toggle;
    }


    // Sub
    public void Run_MovementAction(Vector2 destination)
    {
        Toggle_ActionRunningSignal(true);

        _controller.movement.Move(destination);
        StartCoroutine(MovementAction_Update());
    }
    public IEnumerator MovementAction_Update()
    {
        while (_controller.movement.At_Destination() == false) yield return null;

        Toggle_ActionRunningSignal(false);
    }
}