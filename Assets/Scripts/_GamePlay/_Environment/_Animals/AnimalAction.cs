using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimalAction : MonoBehaviour
{
    [SerializeField] private Animal _controller;

    public virtual bool RunAction()
    {
        return true;
    }
}