using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_Roam : AnimalAction
{
    public override void Run_Action()
    {
        List<Tile> roamTiles = controller.MoveDistance_RangeTiles();
        roamTiles.Remove(controller.movement.tileTracker.data.CurrentTile());

        if (roamTiles.Count <= 0) return;

        Tile roamTile = roamTiles[Random.Range(0, roamTiles.Count)];
        Run_MovementAction(roamTile.Random_BoundPoint());
        
        return;
    }
}
