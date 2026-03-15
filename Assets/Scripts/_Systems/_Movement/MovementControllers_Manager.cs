using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementControllers_Manager : MonoBehaviour
{
    private HashSet<Movement_Controller> _allMovementControllers = new();
    public HashSet<Movement_Controller> allMovementControllers => _allMovementControllers;


    public bool AllMovements_Complete()
    {
        if (_allMovementControllers.Count <= 0) return true;
        
        foreach (Movement_Controller controller in _allMovementControllers)
        {
            if (controller.movementCoroutine == null) continue;
            return false;
        }
        return true;
    }
}
