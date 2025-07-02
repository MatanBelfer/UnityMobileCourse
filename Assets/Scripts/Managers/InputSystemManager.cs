using System.Dynamic;
using UnityEngine;
using UnityEngine.InputSystem;

//TODO class wide refactor 
//move pin and other game logic to another class
//leave only input handlers and event calls

public enum ControlScheme
{
    DragAndDrop = 0,
    TapTap = 1 //tap pin to select, tap a place to make it move there
}

public class InputSystemManager : BaseManager
{
    private PlayerInputActions _inputActions = null;

    [Header("Input Settings")] [SerializeField]
    private ControlScheme controlScheme = ControlScheme.DragAndDrop;

    [SerializeField] private float _maxClickDistance = 0.5f;

    private PinLogic _currentPin;
    private bool _isInputEnabled = false;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnInitialize()
    {
        // Create input actions only once
        if (_inputActions == null)
        {
//            Debug.Log("Creating input actions for InputSystemManager awake");
            _inputActions = new PlayerInputActions();
        }

        LoadControlScheme();
    }

    public void LoadControlScheme()
    {
        controlScheme = (ControlScheme)PlayerPrefs.GetInt("controlScheme");
        EnableInput();
    }

    protected override void OnReset()
    {
        // Keep input enabled during reset, just clear current pin
        _currentPin = null;
        // DisableInput(controlScheme);

        // Re-enable input if it was disabled
        // if (!_isInputEnabled)
        // {
        //     EnableInput(controlScheme);
        // }
    }

    protected override void OnCleanup()
    {
        // Don't disable input during cleanup since this is a core manager
        // that persists across scenes. Just clear current pin.
        _currentPin = null;
    }


    private void EnableInput()
    {
        if (_inputActions == null)
        {
            Debug.Log("Input actions is null");
            return;
        }

        if (!_isInputEnabled)
        {
            _inputActions.PinMovement.Enable();
            //UI related 
            _inputActions.PinMovement.TakeScreenshot.performed += OnTakeScreenshot;
            
            switch (controlScheme)
            {
                case ControlScheme.DragAndDrop:
                    _inputActions.PinMovement.Position.started += OnPositionChanged;
                    _inputActions.PinMovement.Position.performed += OnPositionChanged;
                    _inputActions.PinMovement.ToggleMode.started += OnToggleModePressed;

                    break;
                case ControlScheme.TapTap:
                    _inputActions.PinMovement.Position.started += OnPositionChanged;
                    break;
                default:
                    Debug.Log("Invalid control scheme");
                    break;
            }

            _inputActions.PinMovement.Click.canceled += OnClickEnded;
            _isInputEnabled = true;
            // Debug.Log($"Input enabled for InputSystemManager controlScheme: {controlScheme}");
        }
        else
        {
            Debug.Log("Input already enabled for InputSystemManager");
            DisableInput(controlScheme == ControlScheme.DragAndDrop
                ? ControlScheme.TapTap
                : ControlScheme.DragAndDrop);
            EnableInput();
        }
        
    }

    private void DisableInput(ControlScheme scheme)
    {
        if (_inputActions != null && _isInputEnabled)
        {
            switch (scheme)
            {
                case ControlScheme.DragAndDrop:
                    _inputActions.PinMovement.Position.started -= OnPositionChanged;
                    _inputActions.PinMovement.Position.performed -= OnPositionChanged;
                    _inputActions.PinMovement.ToggleMode.started -= OnToggleModePressed;

                    break;
                case ControlScheme.TapTap:
                    _inputActions.PinMovement.Position.started -= OnPositionChanged;
                    break;
                default:
                    Debug.Log("Invalid control scheme");
                    break;
            }
            _inputActions.PinMovement.Click.canceled -= OnClickEnded;

            _inputActions.PinMovement.Disable();
            _isInputEnabled = false;
            // Debug.Log("Input disabled for InputSystemManager");
        }
    }

    // public void LoadControlScheme(ControlScheme scheme)
    // {
    //     DisableInput(scheme);
    //     controlScheme = (ControlScheme)PlayerPrefs.GetInt("controlScheme");
    //     print(controlScheme.GetName());
    // }

    private void OnTakeScreenshot(InputAction.CallbackContext obj)
    {
        if (ManagersLoader.UI != null)
        {
            StartCoroutine(ManagersLoader.UI.TakeScreenshot());
        }
    }

    private void OnClickEnded(InputAction.CallbackContext context)
    {
        Vector3 endPosition = GetClickWorldPosition();

//        print($"OnClickEnded was called while controlScheme is {controlScheme.GetName()}");
        if (controlScheme == ControlScheme.DragAndDrop)
        {
            HandleDragEnd(endPosition);
        }
        else
        {
            HandleSelectEnd(endPosition);
            // print("OnCLickEnded caused HandleSelectEnd to be called");
        }
    }

    private bool IsOverPin(Vector3 worldPosition)
    {
        PinLogic clickedPin = FindClosestPin(worldPosition);

        if (_currentPin != null && clickedPin != null)
        {
            if (_currentPin != clickedPin)
            {
                return true;
            }
        }

        return false;
    }

    private void OnPositionChanged(InputAction.CallbackContext context)
    {
        Vector3 clickPosition = ScreenToWorldPosition(context.ReadValue<Vector2>());
        PinLogic clickedPin = FindClosestPin(clickPosition);

        if (_currentPin != null)
        {
//            Debug.Log($"Pin found  {_currentPin.name} at click position {clickPosition}");
            if (controlScheme == ControlScheme.DragAndDrop)
            {
                StartDragging(_currentPin);
            }
            else
            {
                HandlePinSelection(_currentPin);
            }
        }
        else
        {
            _currentPin = clickedPin;
        }

        if (controlScheme == ControlScheme.DragAndDrop && _currentPin?.isFollowing == true)
        {
            Vector3 worldPosition = ScreenToWorldPosition(context.ReadValue<Vector2>());
            _currentPin.transform.position = worldPosition;
        }
    }

    private void OnToggleModePressed(InputAction.CallbackContext context)
    {
        ControlScheme newScheme = controlScheme == ControlScheme.DragAndDrop
            ? ControlScheme.TapTap
            : ControlScheme.DragAndDrop;
        SetControlScheme(newScheme);
        // Debug.Log($"Switched to {(_isDragMode ? "DRAG" : "SELECT")} MODE");
    }

    public void SetControlScheme(ControlScheme scheme)
    {
        if (scheme == controlScheme) return;
        controlScheme = scheme;
        ClearCurrentPin();

        // Save the control scheme to PlayerPrefs
        PlayerPrefs.SetInt("controlScheme", (int)scheme);
        PlayerPrefs.Save();
    }

    private void StartDragging(PinLogic pin)
    {
        _currentPin = pin;
        _currentPin.StartFollowingPin();

        // Debug.Log($"Started dragging pin: {pin.name}");
    }

    private void HandlePinSelection(PinLogic clickedPin)
    {
        _currentPin = (_currentPin == clickedPin) ? null : clickedPin;

        Debug.Log(_currentPin == null ? "Pin deselected" : $"Pin selected: {_currentPin.name}");
    }

    private void HandleDragEnd(Vector3 endPosition)
    {
        if (_currentPin != null)
        {
            _currentPin.MovePinToPosition( endPosition, false); // No animation for drag
            _currentPin.StopFollowingPin();
            //   Debug.Log($"Finished dragging pin: {_currentPin.name}");
            _currentPin = null;
        }
    }

    private void HandleSelectEnd(Vector3 endPosition)
    {
        PinLogic clickedPin = FindClosestPin(endPosition);
        Debug.Log($"position: {endPosition}");

        // Fixed null reference exception by checking if pins are null before accessing their names
        string clickedPinName = clickedPin != null ? clickedPin.name : "null";
        string currentPinName = _currentPin != null ? _currentPin.name : "null";
        Debug.Log($"HandleSelectEnd was called with clickedPin: {clickedPinName} and _currentPin: {currentPinName}");

        // Handle tap-tap logic properly
        if (clickedPin != null)
        {
            // Clicked on a pin - select/deselect it
            if (_currentPin == clickedPin)
            {
                // Deselect if clicking the same pin
                _currentPin = null;
                Debug.Log("Pin deselected");
            }
            else
            {
                // Select the new pin
                _currentPin = clickedPin;
                Debug.Log($"Pin selected: {_currentPin.name}");
            }
        }
        else if (_currentPin != null)
        {
            // Clicked on empty space with a pin selected - move the pin
            _currentPin.MovePinToPosition(endPosition, true); // With animation for select
            print("MovePinToPosition Called");
            Debug.Log($"Moved selected pin to: {endPosition}");
            _currentPin = null;
        }
    }

    private void ClearCurrentPin()
    {
        if (_currentPin?.isFollowing == true)
        {
            _currentPin.StopFollowingPin();
        }

        _currentPin = null;
    }

    private Vector3 GetClickWorldPosition()
    {
        if (_inputActions != null)
        {
            return ScreenToWorldPosition(_inputActions.PinMovement.Position.ReadValue<Vector2>());
        }

        return Vector3.zero;
    }

    private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("No main camera found!");
            return Vector3.zero;
        }

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));
        worldPosition.z = 0f;
        return worldPosition;
    }

    private PinLogic FindClosestPin(Vector3 worldPosition)
    {
        PinLogic[] allPins = FindObjectsByType<PinLogic>(FindObjectsSortMode.None);
        PinLogic closestPin = null;
        float closestDistance = float.MaxValue;

        foreach (PinLogic pin in allPins)
        {
            if (pin == null) continue;

            float distance = Vector3.Distance(pin.transform.position, worldPosition);
            if (distance < closestDistance && distance <= _maxClickDistance)
            {
                closestDistance = distance;
                closestPin = pin;
            }
        }

        return closestPin;
    }

    // Only disable input when the object is being destroyed
    private void OnDestroy()
    {
        DisableInput(controlScheme);

        // Dispose of input actions only when the manager is actually destroyed
        if (_inputActions != null)
        {
            _inputActions.Dispose();
            _inputActions = null;
        }
    }
}