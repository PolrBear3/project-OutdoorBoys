using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalTrap : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _damage;
    [SerializeField][Range(0, 10)] private int _stunUpdateCount;


    // MonoBehaviour
    private void Start()
    {
        _placeableItem.placedTile.OnSetPrefab += Activate;
    }

    private void OnDestroy()
    {
        _placeableItem.placedTile.OnSetPrefab -= Activate;
    }


    // Main
    private Animal Damageable_TargetAnimal(GameObject targetPrefab)
    {
        if (targetPrefab.TryGetComponent(out Animal targetAnimal) == false) return null;

        AnimalData data = targetAnimal.data;

        if (data == null) return null;
        if (targetAnimal.Deceased() || data.isOnSight == false) return null;

        return targetAnimal;
    }

    private void Activate(GameObject activatePrefab)
    {
        Animal targetAnimal = Damageable_TargetAnimal(activatePrefab);

        if (targetAnimal == null) return;
        if (targetAnimal.TryGetComponent(out IDamageable damageable) == false) return;

        damageable.InflictDamage(_damage);
        targetAnimal.data.statusStatesData.Register_State(StatusState.stunned, _stunUpdateCount);

        _placeableItem.AnimationDelay_Remove();
    }
}