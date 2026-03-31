using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _damage;

    [Space(10)]
    [SerializeField] private AnimalScrObj[] _activatePriorities;


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
    private Animal Activate_TargetAnimal()
    {
        GameObject eventsPrefab = InGame_Manager.instance.worldMapGenerator.currentMapEventsPrefab;
        if (eventsPrefab.TryGetComponent(out Animals_Manager manager) == false) return null;

        List<Animal> currentAnimals = manager.SpwnedAnimals(_placeableItem.currentTile);
        if (currentAnimals.Count <= 0) return null;

        for (int i = 0; i < _activatePriorities.Length; i++)
        {
            for (int j = 0; j < currentAnimals.Count; j++)
            {
                Animal currentAnimal = currentAnimals[j];
                
                if (currentAnimal.data.animalScrObj != _activatePriorities[i]) continue;
                return currentAnimal;
            }
        }
        return null;
    }
    
    private void Activate()
    {
        Animal activateAnimal = Activate_TargetAnimal();
        if (activateAnimal == null) return;
        
        StartCoroutine(Activate_Update(activateAnimal));
    }
    private IEnumerator Activate_Update(Animal activateAnimal)
    {
        MovementControllers_Manager movementsManager = InGame_Manager.instance.movements;
        while (movementsManager.AllMovements_Complete() == false) yield return null;

        _placeableItem.AnimationDelay_Remove();
        
        AnimalData data = activateAnimal.data;

        data.Update_Health(data.health - _damage);
        activateAnimal.Update_DeceasedState();
    }
}
