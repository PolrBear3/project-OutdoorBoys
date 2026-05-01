using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_RunOff : AnimalAction
{
    public override void Run_Action()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();
        int randDistance = Random.Range(1, controller.data.animalScrObj.moveDistance);

        List<Tile> runOffTiles = controller.MoveDistance_RangeTiles(randDistance);
        runOffTiles.Remove(controller.movement.tileTracker.data.CurrentTile());

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
