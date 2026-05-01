using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_RunOff : AnimalAction
{
    public override void Run_Action()
    {
        if (CheckActions_Complete() == false) return;

        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();

        int randDistance = Random.Range(1, controller.data.animalScrObj.moveDistance);
        List<Tile> runOffTiles = controller.MoveDistance_RangeTiles(randDistance);

        if (runOffTiles.Count <= 0) return;

        runOffTiles.Sort((a, b) =>
        {
            int distA = a.DistanceTo_TargetTile(playerTile);
            int distB = b.DistanceTo_TargetTile(playerTile);
            return distB.CompareTo(distA);
        });

        Run_MovementAction(runOffTiles[0].Random_BoundPoint());
        return;
    }
}
