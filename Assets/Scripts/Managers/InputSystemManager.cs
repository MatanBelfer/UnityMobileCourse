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
        EnableInput();
    }

    protected override void OnReset()
    {
        // Keep input enabled during reset, just clear current pin
        _currentPin = null;
        LoadControlScheme();

        // Re-enable input if it was disabled
        if (!_isInputEnabled)
        {
            EnableInput();
        }
    }

    protected override void OnCleanup()
    {
        // Don't disable input during cleanup since this is a core manager
        // that persists across scenes. Just clear current pin.
        _currentPin = null;
    }

    private void EnableInput()
    {
        if (_inputActions == null) Debug.Log("Input actions is null");

        if (_inputActions != null && !_isInputEnabled)
        {
//            Debug.Log("Enabling input for InputSystemManager");
            _inputActions.PinMovement.Enable();

            //UI related 
            _inputActions.PinMovement.TakeScreenshot.performed += OnTakeScreenshot;

            // Input events
            _inputActions.PinMovement.Click.canceled += OnClickEnded;
            _inputActions.PinMovement.Position.started += OnPositionChanged;
            _inputActions.PinMovement.Position.performed += OnPositionChanged;
            _inputActions.PinMovement.ToggleMode.started += OnToggleModePressed;

            _isInputEnabled = true;
//            Debug.Log("Input enabled for InputSystemManager");
        }
        else
        {
            Debug.LogWarning("Input ");
        }
    }

    private void DisableInput()
    {
        if (_inputActions != null && _isInputEnabled)
        {
            _inputActions.PinMovement.Click.canceled -= OnClickEnded;
            _inputActions.PinMovement.Position.started -= OnPositionChanged;
            _inputActions.PinMovement.Position.performed -= OnPositionChanged;
            _inputActions.PinMovement.ToggleMode.started -= OnToggleModePressed;
            _inputActions.PinMovement.TakeScreenshot.performed -= OnTakeScreenshot;

            _inputActions.PinMovement.Disable();
            _isInputEnabled = false;
            Debug.Log("Input disabled for InputSystemManager");
        }
    }

    public void LoadControlScheme()
    {
        controlScheme = (ControlScheme)PlayerPrefs.GetInt("controlScheme");
        print(controlScheme.GetName());
    }

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
            // Debug.Log($"Pin found at click position {clickPosition}");
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
    }

    private void StartDragging(PinLogic pin)
    {
        _currentPin = pin;
        _currentPin.StartFollowingPin(_currentPin);

        // Debug.Log($"Started dragging pin: {pin.name}");
    }

    private void HandlePinSelection(PinLogic clickedPin)
    {
        _currentPin = (_currentPin == clickedPin) ? null : clickedPin;

        //   Debug.Log(_currentPin == null ? "Pin deselected" : $"Pin selected: {_currentPin.name}");
    }

    private void HandleDragEnd(Vector3 endPosition)
    {
        if (_currentPin != null)
        {
            _currentPin.MovePinToPosition(_currentPin, endPosition, false); // No animation for drag
            _currentPin.StopFollowingPin(_currentPin);
            //   Debug.Log($"Finished dragging pin: {_currentPin.name}");
            _currentPin = null;
        }
    }

    private void HandleSelectEnd(Vector3 endPosition)
    {
        PinLogic clickedPin = FindClosestPin(endPosition);
        print($"HandleSelectEnd was called with clickedPin: {clickedPin} and _currentPin: {_currentPin}");
        if (clickedPin == null && _currentPin != null)
        {
            // Move selected pin to empty space with animation
            _currentPin.MovePinToPosition(_currentPin, endPosition, true); // With animation for select
            print("MovePinToPosition Called");
            //  Debug.Log($"Moved selected pin to: {endPosition}");
            _currentPin = null;
        }
    }

    private void ClearCurrentPin()
    {
        if (_currentPin?.isFollowing == true)
        {
            _currentPin.StopFollowingPin(_currentPin);
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
        DisableInput();

        // Dispose of input actions only when the manager is actually destroyed
        if (_inputActions != null)
        {
            _inputActions.Dispose();
            _inputActions = null;
        }
    }
}