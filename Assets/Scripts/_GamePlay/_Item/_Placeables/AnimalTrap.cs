using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalTrap : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _damage;

    private Coroutine _activationCoroutine;


    // MonoBehaviour
    private void Start()
    {
        _placeableItem.currentTile.OnSetPrefab += Activate;
    }

    private void OnDestroy()
    {
        _placeableItem.currentTile.OnSetPrefab -= Activate;
    }


    // Main
    private IDamageable Damageable_TargetAnimal(GameObject targetPrefab)
    {
        if (targetPrefab.TryGetComponent(out Animal targetAnimal) == false) return null;

        AnimalData data = targetAnimal.data;
        
        if (data == null) return null;
        if (targetAnimal.Deceased() || data.isOnSight == false) return null;

        if (targetAnimal.TryGetComponent(out IDamageable damageable) == false) return null;
        return damageable;
    }

    private void Activate(GameObject activatePrefab)
    {
        IDamageable damageAnimal = Damageable_TargetAnimal(activatePrefab);

        if (damageAnimal == null) return;
        damageAnimal.InflictDamage(_damage);

        _placeableItem.AnimationDelay_Remove();
    }
}