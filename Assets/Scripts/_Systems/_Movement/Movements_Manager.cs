using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movements_Manager : MonoBehaviour
{
    private HashSet<Movement_Controller> _movementControllers = new();
    public HashSet<Movement_Controller> movementControllers => _movementControllers;

    public bool AllMovements_Complete()
    {
        foreach (Movement_Controller controller in _movementControllers)
        {
            if (controller.moveCoroutine != null) return false;
        }
        return true;
    }
}
