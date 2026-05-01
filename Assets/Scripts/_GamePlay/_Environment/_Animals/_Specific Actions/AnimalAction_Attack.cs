using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_Attack : AnimalAction
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _attackDamage;
    [SerializeField][Range(0, 10)] private float _attackIndicationTime;


    // Main
    public override void Run_Action()
    {
        if (CheckActions_Complete() == false) return;

        List<Tile> attackTiles = controller.MoveDistance_RangeTiles();

        for (int i = 0; i < attackTiles.Count; i++)
        {
            List<GameObject> currentPrefabs = attackTiles[i].All_CurrentPrefabs();

            for (int j = currentPrefabs.Count - 1; j >= 0; j--)
            {
                if (currentPrefabs[j].TryGetComponent(out IDamageable damageable) == false) continue;
                damageable.InflictDamage(_attackDamage);
            }
        }

        Toggle_ActionRunningSignal(true);
        StartCoroutine(AttackIndication_Update());
    }

    private IEnumerator AttackIndication_Update()
    {
        controller.completedActions.Add(this);
        Toggle_ActionRunningSignal(false);

        yield break;
    }
}