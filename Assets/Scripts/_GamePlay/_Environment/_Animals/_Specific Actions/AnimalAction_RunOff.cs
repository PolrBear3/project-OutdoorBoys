using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_RunOff : AnimalAction
{
    public override bool RunAction()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();

        List<Tile> runOffTiles = controller.MoveDistance_RangeTiles();
        if (runOffTiles.Count <= 0) return false;

        runOffTiles.Sort((a, b) =>
        {
            int distA = a.DistanceTo_TargetTile(playerTile);
            int distB = b.DistanceTo_TargetTile(playerTile);
            return distB.CompareTo(distA);
        });

        Toggle_ActionRunningSignal(true);

        controller.movement.Move(runOffTiles[0]);
        StartCoroutine(RunOff_Update());

        return true;
    }

    private IEnumerator RunOff_Update()
    {
        while (controller.movement.At_Destination() == false) yield return null;
        Toggle_ActionRunningSignal(false);
    }
}
