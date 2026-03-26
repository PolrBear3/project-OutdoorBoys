using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;


    // MonoBehaviour
    private void Awake()
    {
        InGame_Manager.instance.time.OnTimeUpdate += Activate;
    }

    private void OnDestroy()
    {
        InGame_Manager.instance.time.OnTimeUpdate -= Activate;
    }


    // Main
    private void Activate()
    {
        Tile currentTile = _placeableItem.currentTile;
        List<Animal> currentAnimals = InGame_Manager.instance.animals.SpwnedAnimals(currentTile);

        if (currentAnimals.Count <= 0) return;

        Animal activateAnimal = currentAnimals[Random.Range(0, currentAnimals.Count)];
        // ?

        _placeableItem.AnimationDelay_Remove();
    }
}
