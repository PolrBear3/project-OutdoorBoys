using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimalData
{
    private AnimalScrObj _animalScrObj;
    public AnimalScrObj animalScrObj => _animalScrObj;

    private StatusStates_Data _statusStatesData;
    public StatusStates_Data statusStatesData => _statusStatesData;

    private int _health;
    public int health => _health;

    private int _trailMarkCount;
    public int trailMarkCount => _trailMarkCount;

    private bool _isOnSight;
    public bool isOnSight => _isOnSight;


    // Constructors
    public AnimalData(AnimalScrObj setAnimal, int setHealth, int setTrailMarkCount)
    {
        _animalScrObj = setAnimal;
        _statusStatesData = new();
        _health = setHealth;
        _trailMarkCount = setTrailMarkCount;
    }


    // Data
    public int Update_Health(int updateValue)
    {
        _health = Mathf.Max(0, updateValue);
        return _health;
    }

    public void Decrease_TrailMarkCount(int decreaseValue)
    {
        _trailMarkCount = Mathf.Max(0, _trailMarkCount - decreaseValue);
        _isOnSight = _trailMarkCount <= 0;
    }
}
