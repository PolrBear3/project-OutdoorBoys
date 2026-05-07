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
        InGame_Manager manager = InGame_Manager.instance;

        manager.time.Register(ActionUpdateBus.StartUpdate, Activate);
        manager.player.movement.tileTracker.Register(ActionUpdateBus.StartUpdate, Activate);
    }

    private void OnDestroy()
    {
        InGame_Manager manager = InGame_Manager.instance;

        manager.time.UnRegister(ActionUpdateBus.StartUpdate, Activate);
        manager.player.movement.tileTracker.UnRegister(ActionUpdateBus.StartUpdate, Activate);
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
                if (currentAnimal.movement.tileTracker.data.CurrentTile() != _placeableItem.currentTile) continue;

                return currentAnimal;
            }
        }
        return null;
    }

    private void Activate()
    {
        StartCoroutine(Activate_Delay());
    }
    private void Activate(Tile _)
    {
        Activate();
    }

    private IEnumerator Activate_Delay()
    {
        Time_Manager time = InGame_Manager.instance.time;
        while (time.TimeUpdateActions_Running()) yield return null;

        Animal activateAnimal = Activate_TargetAnimal();
        if (activateAnimal == null || activateAnimal.data.isOnSight == false) yield break;

        if (activateAnimal.TryGetComponent(out IDamageable damageable) == false) yield break;
        damageable.InflictDamage(_damage);

        _placeableItem.AnimationDelay_Remove();
    }
}