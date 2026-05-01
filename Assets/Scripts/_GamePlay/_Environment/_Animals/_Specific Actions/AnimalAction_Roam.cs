using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_Roam : AnimalAction
{
    public override void Run_Action()
    {
        if (CheckActions_Complete() == false) return;

        Tile roamTile = controller.MoveDistance_RangeTile(false);
        if (roamTile == null) return;

        Run_MovementAction(roamTile.Random_BoundPoint());
        return;
    }
}
