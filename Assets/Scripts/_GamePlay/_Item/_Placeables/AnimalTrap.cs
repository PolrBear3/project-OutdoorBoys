using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalTrap : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _damage;

    [Space(10)]
    [SerializeField] private AnimalScrObj[] _activatePriorities;


    // MonoBehaviour
    private void Awake()
    {
        InGame_Manager.instance.time.Register(TimeUpdateBus.StartUpdate, Activate);
    }

    private void OnDestroy()
    {
        InGame_Manager.instance.time.UnRegister(TimeUpdateBus.StartUpdate, Activate);
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
                if (currentAnimal.Deceased()) continue;

                if (currentAnimal.data.animalScrObj != _activatePriorities[i]) continue;
                return currentAnimal;
            }
        }
        return null;
    }

    private void Activate()
    {
        StartCoroutine(Activate_Delay());
    }
    private IEnumerator Activate_Delay()
    {
        /*
        MovementControllers_Manager movementsManager = InGame_Manager.instance.movements;
        while (movementsManager.AllMovements_Complete() == false) yield return null;
        */

        Animal activateAnimal = Activate_TargetAnimal();
        if (activateAnimal == null || activateAnimal.data.isOnSight == false) yield break;

        if (activateAnimal.TryGetComponent(out IDamageable damageable) == false) yield break;
        damageable.InflictDamage(_damage);

        // Movement_Controller movement = activateAnimal.movement;
        // activateAnimal.movement.Update_CurrentState(MovementState.stunned, movement.CurrentState_Count(MovementState.stunned) + 1);

        _placeableItem.AnimationDelay_Remove();
    }
}