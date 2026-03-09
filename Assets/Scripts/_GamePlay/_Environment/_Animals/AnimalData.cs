using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimalData
{
    private AnimalScrObj _animalScrObj;
    public AnimalScrObj animalScrObj => _animalScrObj;

    private int _health;
    public int health => _health;    

    private int _trailMarkCount;
    public int trailMarkCount => _trailMarkCount;

    private bool _isOnSight;
    public bool isOnSight => _isOnSight;

    private int _onSightTimeCount;
    public int onSightTimeCount => _onSightTimeCount;


    // Constructors
    public AnimalData(AnimalScrObj setAnimal, int setHealth, int setTrailMarkCount)
    {
        _animalScrObj = setAnimal;
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
        _trailMarkCount -= decreaseValue;
        _isOnSight = _trailMarkCount <= 0;
    }

    public void Update_OnSightTimeCount(int updateValue)
    {
        _onSightTimeCount += updateValue;
    }
}
