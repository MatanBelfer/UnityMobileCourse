using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PinInputSystem : MonoBehaviour
{
    public PlayerInputActions inputActions;
    
    [Header("Input Mode Settings")]
    [SerializeField] private bool isDragMode = true; // Toggle between drag and select mode
    
    [Header("Drag Mode Settings")]
    [SerializeField] private float maxClickDistance = 1f; // Max distance to detect pin clicks
    
    private bool isInputActive = false;
    private Vector2 currentInputPosition;
    private PinLogic currentlyDraggedPin = null;
    private PinLogic selectedPin = null;
    
    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        
        inputActions.PinMovement.Click.started += OnClickStarted;
        inputActions.PinMovement.Click.canceled += OnClickCanceled;
        inputActions.PinMovement.Cancel.started += OnCancelStarted;
        inputActions.PinMovement.Position.performed += OnPositionChanged;
        inputActions.PinMovement.ToggleMode.started += OnToggleModePressed;
    }

    private void OnDisable()
    {
        inputActions.PinMovement.Click.started -= OnClickStarted;
        inputActions.PinMovement.Click.canceled -= OnClickCanceled;
        inputActions.PinMovement.Cancel.started -= OnCancelStarted;
        inputActions.PinMovement.Position.performed -= OnPositionChanged;
        inputActions.PinMovement.ToggleMode.started -= OnToggleModePressed;
        
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }

    private void OnToggleModePressed(InputAction.CallbackContext context)
    {
        ToggleInputMode();
    }

    private void ToggleInputMode()
    {
        isDragMode = !isDragMode;
        ClearAllStates();
        
        string modeText = isDragMode ? "DRAG MODE" : "SELECT MODE";
        Debug.Log($"Switched to {modeText}");
    }

    private void OnCancelStarted(InputAction.CallbackContext context)
    {
        Debug.Log("Cancel pressed");
        ClearAllStates();
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        isInputActive = true;
        Vector3 worldPosition = ScreenToWorldPosition(currentInputPosition);
        
        if (isDragMode)
        {
            HandleDragModeStart(worldPosition);
        }
        else
        {
            HandleSelectModeStart(worldPosition);
        }
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        isInputActive = false;
        
        if (isDragMode)
        {
            HandleDragModeEnd();
        }
        else
        {
            HandleSelectModeEnd();
        }
    }

    private void OnPositionChanged(InputAction.CallbackContext context)
    {
        currentInputPosition = context.ReadValue<Vector2>();
        
        if (isDragMode && isInputActive && currentlyDraggedPin != null && currentlyDraggedPin.isFollowing)
        {
            Vector3 worldPosition = ScreenToWorldPosition(currentInputPosition);
            currentlyDraggedPin.transform.position = worldPosition;
        }
    }

    private void HandleDragModeStart(Vector3 worldPosition)
    {
        Debug.Log($"Drag mode: Click started at {worldPosition}");
        
        PinLogic clickedPin = FindClosestPin(worldPosition);
        
        if (clickedPin != null && !clickedPin.isFollowing)
        {
            // Start dragging immediately - ONLY if we clicked on a pin
            currentlyDraggedPin = clickedPin;
            selectedPin = clickedPin;
            StartFollowingPin(clickedPin);
            Debug.Log($"Started dragging pin: {clickedPin.name}");
        }
        else
        {
            // In drag mode, clicking on empty space does NOTHING
            Debug.Log("Drag mode: Clicked on empty space - no action");
        }
    }
    
    private void HandleDragModeEnd()
    {
        Debug.Log("Drag mode: Click ended");
        
        if (currentlyDraggedPin != null && currentlyDraggedPin.isFollowing)
        {
            StopFollowingPin(currentlyDraggedPin);
            DeselectPin(currentlyDraggedPin);
            currentlyDraggedPin = null;
        }
    }
    
    private void HandleSelectModeStart(Vector3 worldPosition)
    {
        Debug.Log($"Select mode: Click started at {worldPosition}");
        
        PinLogic clickedPin = FindClosestPin(worldPosition);
        
        if (clickedPin != null && !clickedPin.isFollowing)
        {
            // Handle pin selection
            if (selectedPin == clickedPin)
            {
                // Clicking on already selected pin - deselect it
                DeselectPin(clickedPin);
            }
            else
            {
                // Select new pin
                if (selectedPin != null)
                {
                    DeselectPin(selectedPin);
                }
                SelectPin(clickedPin);
            }
        }
        else if (clickedPin == null && selectedPin != null)
        {
            // Clicked on empty space with a pin selected - move the pin there
            MovePinToPosition(selectedPin, worldPosition);
            DeselectPin(selectedPin);
        }
        else
        {
            // Clicked on empty space with no pin selected - do nothing
            Debug.Log("Select mode: Clicked on empty space with no pin selected");
        }
    }
    
    private void HandleSelectModeEnd()
    {
        Debug.Log("Select mode: Click ended");
        // In select mode, we don't need to do anything on click end
    }
    
    private void ClearAllStates()
    {
        if (selectedPin != null)
        {
            DeselectPin(selectedPin);
        }
        
        if (currentlyDraggedPin != null && currentlyDraggedPin.isFollowing)
        {
            StopFollowingPin(currentlyDraggedPin);
            currentlyDraggedPin = null;
        }
        
        isInputActive = false;
    }

    private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("No main camera found!");
            return Vector3.zero;
        }
        
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));
        worldPosition.z = 0f;
        return worldPosition;
    }

    private void SelectPin(PinLogic pin)
    {
        selectedPin = pin;
        Debug.Log($"Selected pin: {pin.name} at position: {pin.transform.position}");
    }

    private void DeselectPin(PinLogic pin)
    {
        if (pin == null) return;
        
        selectedPin = null;
        Debug.Log($"Deselected pin: {pin.name}");
    }

    private void MovePinToPosition(PinLogic pin, Vector3 worldPosition)
    {
        if (pin == null || pin.gridManager == null) return;
        
        Debug.Log($"Moving pin {pin.name} to position: {worldPosition}");
        
        Transform landingPoint = pin.gridManager.GetClosestPoint(worldPosition);
        if (landingPoint != null)
        {
            pin.transform.parent = landingPoint;
            pin.transform.localPosition = Vector3.zero;
            
            if (GeometricRubberBand.Instance != null)
            {
                GeometricRubberBand.Instance.UpdateMovingPin(pin.transform, GeometricRubberBand.MovingPinStatus.NotMoving);
            }
        }
    }

    private void StartFollowingPin(PinLogic pin)
    {
        if (pin == null) return;
        
        pin.isFollowing = true;
        pin.transform.parent = null;
        
        if (GeometricRubberBand.Instance != null)
        {
            GeometricRubberBand.Instance.UpdateMovingPin(pin.transform, GeometricRubberBand.MovingPinStatus.Moving);
        }
    }

    private void StopFollowingPin(PinLogic pin)
    {
        if (pin == null || pin.gridManager == null) return;
        
        Transform landingPoint = pin.gridManager.GetClosestPoint(pin.transform.position);
        if (landingPoint != null)
        {
            pin.transform.parent = landingPoint;
            pin.transform.localPosition = Vector3.zero;
        }

        pin.isFollowing = false;
        
        if (GeometricRubberBand.Instance != null)
        {
            GeometricRubberBand.Instance.UpdateMovingPin(pin.transform, GeometricRubberBand.MovingPinStatus.NotMoving);
        }
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
            if (distance < closestDistance && distance <= maxClickDistance)
            {
                closestDistance = distance;
                closestPin = pin;
            }
        }

        return closestPin;
    }

    public void ClearSelection()
    {
        ClearAllStates();
    }

    public void SetDragMode(bool dragMode)
    {
        isDragMode = dragMode;
        ClearAllStates();
        
        string modeText = isDragMode ? "DRAG MODE" : "SELECT MODE";
        Debug.Log($"Mode set to {modeText}");
    }

    public bool GetCurrentMode()
    {
        return isDragMode;
    }

    [ContextMenu("Toggle Input Mode")]
    public void ToggleInputModeFromMenu()
    {
        ToggleInputMode();
    }

    [ContextMenu("Debug Current State")]
    public void DebugCurrentState()
    {
        Debug.Log($"Input Mode: {(isDragMode ? "DRAG" : "SELECT")}");
        Debug.Log($"Selected Pin: {(selectedPin?.name ?? "None")}");
        Debug.Log($"Dragged Pin: {(currentlyDraggedPin?.name ?? "None")}");
        Debug.Log($"Is Input Active: {isInputActive}");
    }
}