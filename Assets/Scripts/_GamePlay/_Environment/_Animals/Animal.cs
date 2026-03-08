using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animation;

    [SerializeField] private Movement_Controller _movement;
    public Movement_Controller movement => _movement;


    private AnimalData _data;
    public AnimalData data => _data;


    // Data
    public void Set_Data(AnimalScrObj setAnimal)
    {
        _data = new(setAnimal);
    }
}
