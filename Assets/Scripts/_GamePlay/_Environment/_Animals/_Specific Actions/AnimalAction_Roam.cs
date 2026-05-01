using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_Roam : AnimalAction
{
    public override void Run_Action()
    {
        Tile roamTile = controller.MoveDistanceRange_RandomTile(false);
        if (roamTile == null) return;

        Run_MovementAction(roamTile.Random_BoundPoint());
        return;
    }
}
