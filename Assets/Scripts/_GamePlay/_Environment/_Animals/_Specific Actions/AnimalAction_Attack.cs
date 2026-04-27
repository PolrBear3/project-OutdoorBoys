using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_Attack : AnimalAction
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _attackDamage;
    [SerializeField][Range(0, 10)] private float _attackIndicationTime;
    
    public override bool RunAction()
    {
        List<Tile> attackTiles = controller.MoveDistance_RangeTiles();
        if (attackTiles.Count <= 0) return false;

        for (int i = 0; i < attackTiles.Count; i++)
        {
            List<GameObject> currentPrefabs = attackTiles[i].All_CurrentPrefabs();

            for (int j = currentPrefabs.Count - 1; j >= 0 ; j--)
            {
                if (currentPrefabs[j].TryGetComponent(out IDamageable damageable) == false) continue;
                damageable.InflictDamage(_attackDamage);
            }
        }

        Toggle_ActionRunningSignal(true);
        StartCoroutine(AttackIndication_Update(attackTiles));

        return true;
    }

    private IEnumerator AttackIndication_Update(List<Tile> attackIndicateTiles)
    {
        Tile_Indicator tileIndicator = controller.tileIndicator;

        foreach (Tile attackTile in attackIndicateTiles)
        {
            tileIndicator.Set_Indicator(attackTile);
        }
        tileIndicator.Toggle_CurrentIndicators(true);

        yield return new WaitForSeconds(_attackIndicationTime);
        
        tileIndicator.Clear_CurrentIndicators();
        Toggle_ActionRunningSignal(false);
    }
}