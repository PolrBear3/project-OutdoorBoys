using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAction_Roam : AnimalAction
{
    public override bool RunAction()
    {
        Tile roamTile = controller.MoveDistance_RangeTile(false);
        if (roamTile == null) return false;

        Toggle_ActionRunningSignal(true);

        controller.movement.Move(roamTile.Random_BoundPoint());
        StartCoroutine(MovementAction_Update());

        Run_MovementAction(roamTile.Random_BoundPoint());
        return true;
    }
}
