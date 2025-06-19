using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemManager : MonoBehaviour
{
    private PlayerInputActions _inputActions;
    public static InputSystemManager Instance { get; private set; }

    [Header("Input Settings")]
    [SerializeField] private bool _isDragMode = true; // More intuitive naming
    [SerializeField] private float _maxClickDistance = 0.5f;

    private PinLogic _currentPin;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _inputActions = new PlayerInputActions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        _inputActions.PinMovement.Enable();
        _inputActions.PinMovement.Click.started += OnClickStarted;
        _inputActions.PinMovement.Click.canceled += OnClickEnded;
        _inputActions.PinMovement.Position.performed += OnPositionChanged;
        _inputActions.PinMovement.ToggleMode.started += OnToggleModePressed;
    }

    private void OnDisable()
    {
        if (_inputActions != null)
        {
            _inputActions.PinMovement.Click.started -= OnClickStarted;
            _inputActions.PinMovement.Click.canceled -= OnClickEnded;
            _inputActions.PinMovement.Position.performed -= OnPositionChanged;
            _inputActions.PinMovement.ToggleMode.started -= OnToggleModePressed;
            _inputActions.PinMovement.Disable();
        }
    }

    private void OnDestroy()
    {
        _inputActions?.Dispose();
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        Vector3 clickPosition = GetClickWorldPosition();
        PinLogic clickedPin = FindClosestPin(clickPosition);

        if (clickedPin != null)
        {
            if (_isDragMode)
            {
                StartDragging(clickedPin);
            }
            else
            {
                HandlePinSelection(clickedPin);
            }
        }
    }

    private void OnClickEnded(InputAction.CallbackContext context)
    {
        Vector3 endPosition = GetClickWorldPosition();

        if (_isDragMode)
        {
            HandleDragEnd(endPosition);
        }
        else
        {
            HandleSelectEnd(endPosition);
        }
    }

    private void OnPositionChanged(InputAction.CallbackContext context)
    {
        if (_isDragMode && _currentPin?.isFollowing == true)
        {
            Vector3 worldPosition = ScreenToWorldPosition(context.ReadValue<Vector2>());
            _currentPin.transform.position = worldPosition;
        }
    }

    private void OnToggleModePressed(InputAction.CallbackContext context)
    {
        _isDragMode = !_isDragMode;
        ClearCurrentPin();
        Debug.Log($"Switched to {(_isDragMode ? "DRAG" : "SELECT")} MODE");
    }

    // Simplified mode-specific methods
    private void StartDragging(PinLogic pin)
    {
        _currentPin = pin;
        _currentPin.StartFollowingPin(_currentPin);
    }

    private void HandlePinSelection(PinLogic clickedPin)
    {
        _currentPin = (_currentPin == clickedPin) ? null : clickedPin;
    }

    private void HandleDragEnd(Vector3 endPosition)
    {
        if (_currentPin != null)
        {
            _currentPin.MovePinToPosition(_currentPin, endPosition);
            _currentPin.StopFollowingPin(_currentPin);
            _currentPin = null;
        }
    }

    private void HandleSelectEnd(Vector3 endPosition)
    {
        PinLogic clickedPin = FindClosestPin(endPosition);
        
        if (clickedPin == null && _currentPin != null)
        {
            // Move selected pin to empty space
            _currentPin.MovePinToPosition(_currentPin, endPosition);
            _currentPin.StopFollowingPin(_currentPin);
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
        return ScreenToWorldPosition(_inputActions.PinMovement.Position.ReadValue<Vector2>());
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
}