using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenu_InputController : MonoBehaviour
{
    public Action<Vector2> OnNavigate;

    public Action OnSelect;
    public Action OnHoldSelect;
    public Action OnSelectEnd;

    public Action OnExit;
    
    
    // Component
    public void Toggle_Input(bool toggle)
    {
        Input_Controller inputController = Input_Controller.instance;
        List<UIMenu_InputController> toggledMenus = inputController.toggledMenuInputs;
 
        if (toggle && toggledMenus.Contains(this) == false)
        {
            toggledMenus.Add(this);
            inputController.Update_ActionMap(1);
            return;
        }
        toggledMenus.Remove(this);

        if (toggledMenus.Count > 0) return;
        inputController.Update_ActionMap(0);
    }
}