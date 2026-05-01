using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_Follow : AnimalAction
{
    public override void Run_Action()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();
        if (controller.movement.tileTracker.data.CurrentTile().DistanceTo_TargetTile(playerTile) <= 1) return;

        List<Tile> runOffTiles = controller.MoveDistance_RangeTiles();
        if (runOffTiles.Count <= 0) return;

        runOffTiles.Sort((a, b) =>
        {
            int distA = a.DistanceTo_TargetTile(playerTile);
            int distB = b.DistanceTo_TargetTile(playerTile);
            return distA.CompareTo(distB);
        });

        Run_MovementAction(runOffTiles[0].Random_BoundPoint());
        return;
    }
}