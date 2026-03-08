using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimalData
{
    private AnimalScrObj _animalScrObj;
    public AnimalScrObj animalScrObj => _animalScrObj;

    private int _trailTrackCollectCount;
    public int trailTrackCollectCount => _trailTrackCollectCount;


    // Constructors
    public AnimalData(AnimalScrObj setAnimal)
    {
        _animalScrObj = setAnimal;
    }


    // Data
    public void Collect_TrailTrack()
    {
        _trailTrackCollectCount--;
    }
}
