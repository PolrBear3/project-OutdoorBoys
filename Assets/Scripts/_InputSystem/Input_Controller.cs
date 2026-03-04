using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;

public class Input_Controller : MonoBehaviour
{
    public static Input_Controller instance;

    
    [Space(20)]
    [SerializeField] private PlayerInput _playerInput;
    public PlayerInput playerInput => _playerInput;

    [SerializeField] private ControlScheme_ScrObj[] _schemes;


    // Current Datas
    private string _currentSchemeName;

    private ControlScheme_ScrObj _currentControlScheme;
    public ControlScheme_ScrObj currentControlScheme => _currentControlScheme;

    private List<string> _actionMaps = new();
    private HashSet<Guid> _inputGateIDs = new();

    private List<UIMenu_InputController> _toggledMenuInputs = new();
    public List<UIMenu_InputController> toggledMenuInputs => _toggledMenuInputs;


    // Observers
    public Action OnActionMapUpdate;
    public Action OnSchemeUpdate;

    public Action OnAnyInput;


    // Action Map inGame
    public Action<Vector2> OnMovement;
    public Action<Vector2> OnCursorControl;

    public Action OnLeftClickStart;
    public Action OnLeftClick;
    public Action OnHoldLeftClick;

    public Action OnRightClickStart;
    public Action OnRightClick;
    public Action OnHoldRightClick;

    public Action OnInteractStart;
    public Action OnInteract;
    public Action OnHoldInteract;

    public Action OnAction1;
    public Action OnAction2;

    public Action OnCancel;


    // MonoBehaviour
    private void Awake()
    {
        instance = this;

        for (int i = 0; i < _playerInput.actions.actionMaps.Count; i++)
        {
            _actionMaps.Add(_playerInput.actions.actionMaps[i].name);
        }
  
        Handle_SchemeUpdate(_playerInput);
        _playerInput.onControlsChanged += Handle_SchemeUpdate;
    }

    private void Update()
    {
        CurrentScheme_Update();
    }
    
    private void OnDestroy()
    {
        _playerInput.onControlsChanged -= Handle_SchemeUpdate;
    }


    // Action Map
    public void Update_ActionMap(int indexNum)
    {
        if (indexNum < 0 || indexNum >= _actionMaps.Count) return;

        indexNum = Mathf.Clamp(indexNum, 0, _actionMaps.Count - 1);
        string mapName = _actionMaps[indexNum];

        _playerInput.SwitchCurrentActionMap(mapName);

        OnActionMapUpdate?.Invoke();
    }

    public int Current_ActionMapNum()
    {
        string currentMapName = _playerInput.currentActionMap.name;

        for (int i = 0; i < _actionMaps.Count; i++)
        {
            if (currentMapName != _actionMaps[i]) continue;
            return i;
        }
        return 0;
    }


    // Scheme
    public void Update_CurrentScheme(string schemeName)
    {
        _currentControlScheme = ControlScheme(schemeName);

        if (_currentControlScheme == null)
        {
            Debug.Log("Update Scheme Not Found!");
            return;
        }

        OnSchemeUpdate?.Invoke();

        Debug.Log("_currentScheme: " + _currentControlScheme.name + "/ _playerInput.currentControlScheme: " + _playerInput.currentControlScheme);
    }

    public void Update_EmojiAsset(TextMeshProUGUI text)
    {
        text.spriteAsset = _currentControlScheme.emojiAsset;
    }


    // Scheme Auto Update
    private void CurrentScheme_Update()
    {
        if (_currentSchemeName == _playerInput.currentControlScheme) return;
        _currentSchemeName = _playerInput.currentControlScheme;
        
        Update_CurrentScheme(_currentSchemeName);
    }
    
    private void Handle_SchemeUpdate(PlayerInput playerInput)
    {
        Update_CurrentScheme(_playerInput.currentControlScheme);
    }


    // Datas
    public InputActionReference ActionReference(string actionName)
    {
        ActionKey_Data[] datas = _currentControlScheme.actionKeyDatas;

        for (int i = 0; i < datas.Length; i++)
        {
            if (datas[i].actionRef.action.name != actionName) continue;
            return datas[i].actionRef;
        }

        return null;
    }

    private ControlScheme_ScrObj ControlScheme(string name)
    {
        for (int i = 0; i < _schemes.Length; i++)
        {
            if (_schemes[i].schemeName != name) continue;
            return _schemes[i];
        }
        return null;
    }

    public GameObject CurrentScheme_ActionKey(InputActionReference reference)
    {
        ActionKey_Data[] datas = _currentControlScheme.actionKeyDatas;

        for (int i = 0; i < datas.Length; i++)
        {
            if (reference != datas[i].actionRef) continue;
            return datas[i].actionKey;
        }
        return null;
    }


    // Input Actions
    public void AnyInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        OnAnyInput?.Invoke();
    }


    // Action Map inGame
    public void CursorControl(InputAction.CallbackContext context)
    {
        // if (_isHolding) return;
        if (!context.performed) return;

        Vector2 positionInput = context.ReadValue<Vector2>();
        OnCursorControl?.Invoke(positionInput);
    }

    public void LeftClick(InputAction.CallbackContext context)
    {
        Guid actionID = context.action.id;

        if (context.started && _inputGateIDs.Add(actionID))
        {
            OnLeftClickStart?.Invoke();
        }
        
        if (!context.performed) return;
        _inputGateIDs.Remove(actionID);

        if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
        {
            OnHoldLeftClick?.Invoke();
            return;
        }
        OnLeftClick?.Invoke();
    }

    public void RightClick(InputAction.CallbackContext context)
    {
        Guid actionID = context.action.id;

        if (context.started && _inputGateIDs.Add(actionID))
        {
            OnRightClickStart?.Invoke();
        }

        if (!context.performed) return;
        _inputGateIDs.Remove(actionID);

        if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
        {
            OnHoldRightClick?.Invoke();
            return;
        }
        OnRightClick?.Invoke();
    }


    public void Movement(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 directionInput = context.ReadValue<Vector2>();
        OnMovement?.Invoke(directionInput);
    }

    public void Interact(InputAction.CallbackContext context)
    {
        Guid actionID = context.action.id;
        
        if (context.started && _inputGateIDs.Add(actionID)) OnInteractStart?.Invoke();
        if (!context.performed) return;

        _inputGateIDs.Remove(actionID);

        if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
        {
            OnHoldInteract?.Invoke();
            return;
        }
        OnInteract?.Invoke();
    }

    public void Action1(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;
        OnAction1?.Invoke();
    }

    public void Action2(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;
        OnAction2?.Invoke();
    }

    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.performed == false) return;
        OnCancel?.Invoke();
    }


    // Action Map uiMenu
    private bool UIMenuInput_Toggled(out UIMenu_InputController recentInput)
    {
        if (_toggledMenuInputs.Count <= 0)
        {
            recentInput = null;
            return false;
        }
        recentInput = _toggledMenuInputs[_toggledMenuInputs.Count - 1];
        return true;
    }


    public void Navigate(InputAction.CallbackContext context)
    {
        if (UIMenuInput_Toggled(out UIMenu_InputController recentInput) == false) return;
        if (!context.performed) return;

        Vector2 directionInput = context.ReadValue<Vector2>();
        recentInput.OnNavigate?.Invoke(directionInput);
    }

    public void Select(InputAction.CallbackContext context)
    {
        if (UIMenuInput_Toggled(out UIMenu_InputController recentInput) == false) return;

        Guid actionID = context.action.id;

        if (context.started && _inputGateIDs.Add(actionID)) recentInput.OnSelectStart?.Invoke();
        if (!context.performed) return;

        _inputGateIDs.Remove(actionID);

        if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
        {
            recentInput.OnHoldSelect?.Invoke();
            return;
        }
        recentInput.OnSelect?.Invoke();
    }

    public void Exit(InputAction.CallbackContext context)
    {
        if (UIMenuInput_Toggled(out UIMenu_InputController recentInput) == false) return;
        if (context.performed == false) return;

        recentInput.OnExit?.Invoke();
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(Input_Controller))]
public class Input_Controller_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        Input_Controller controller = (Input_Controller)target;

        base.OnInspectorGUI();
        serializedObject.Update();

        GUILayout.Space(60);

        if (GUILayout.Button("Toggle Scheme"))
        {
            if (controller.currentControlScheme.schemeName == "PC")
            {
                controller.Update_CurrentScheme("GamePad");
                return;
            }
            controller.Update_CurrentScheme("PC");
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif