using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action OnEnter;
    public Action OnExit;
    public Action<bool> OnPointerState;

    private bool _pointerDetected;
    public bool pointerDetected => _pointerDetected;


    private void OnDisable()
    {
        OnPointerExit();
    }


    // EventSystems
    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointerDetected = true;

        OnEnter?.Invoke();
        OnPointerState?.Invoke(_pointerDetected);
    }

    private void OnPointerExit()
    {
        if (_pointerDetected == false) return;
        _pointerDetected = false;

        OnExit?.Invoke();
        OnPointerState?.Invoke(_pointerDetected);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExit();
    }
}
