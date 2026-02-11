using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Action OnEnter;
    public Action OnExit;
    public Action OnClick;

    private bool _pointerDetected;
    public bool pointerDetected => _pointerDetected;


    // EventSystems
    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointerDetected = true;
        OnEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerDetected = false;
        OnExit?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }
}
