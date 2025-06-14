using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PinInputSystem : MonoBehaviour
{
    public PlayerInputActions inputActions;
    
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
        
        // Subscribe to click events
        inputActions.PinMovement.Click.started += OnClickStarted;
        inputActions.PinMovement.Click.canceled += OnClickCanceled;
        
        // Subscribe to position updates
        inputActions.PinMovement.Position.performed += OnPositionChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from click events
        inputActions.PinMovement.Click.started -= OnClickStarted;
        inputActions.PinMovement.Click.canceled -= OnClickCanceled;
        
        // Unsubscribe from position updates
        inputActions.PinMovement.Position.performed -= OnPositionChanged;
        
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        isInputActive = true;
        Debug.Log("Click started");
        
        // Convert screen position to world position
        Vector3 worldPosition = ScreenToWorldPosition(currentInputPosition);
        HandleInputStart(worldPosition);
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        isInputActive = false;
        Debug.Log("Click ended");
        
        Vector3 worldPosition = ScreenToWorldPosition(currentInputPosition);
        HandleInputEnd(worldPosition);
    }

    private void OnPositionChanged(InputAction.CallbackContext context)
    {
        currentInputPosition = context.ReadValue<Vector2>();
        
        // Only handle position changes when input is active (clicked/touched)
        if (isInputActive && currentlyDraggedPin != null)
        {
            Vector3 worldPosition = ScreenToWorldPosition(currentInputPosition);
            HandleInputMove(worldPosition);
        }
    }

    private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));
        worldPosition.z = 0f;
        return worldPosition;
    }

    private void HandleInputStart(Vector3 worldPosition)
    {
        Debug.Log($"Input started at world position: {worldPosition}");
        
        // Find the closest pin at click position
        PinLogic clickedPin = FindClosestPin(worldPosition);
        
        if (clickedPin != null && !clickedPin.isFollowing)
        {
            // If there's already a selected pin and we clicked on a different pin
            if (selectedPin != null && selectedPin != clickedPin)
            {
                // Deselect the previous pin
                DeselectPin(selectedPin);
            }
            
            // If we clicked on the same pin that's already selected, start dragging
            if (selectedPin == clickedPin)
            {
                currentlyDraggedPin = clickedPin;
                StartFollowingPin(clickedPin);
                DeselectPin(clickedPin); // Clear selection when dragging starts
            }
            else
            {
                // Select the new pin
                SelectPin(clickedPin);
            }
        }
        else if (clickedPin == null && selectedPin != null)
        {
            // Clicked on empty space with a pin selected - move the pin there
            MovePinToPosition(selectedPin, worldPosition);
            DeselectPin(selectedPin);
        }
        else if (clickedPin == null && selectedPin == null)
        {
            // Clicked on empty space with no pin selected - do nothing
            Debug.Log("Clicked on empty space with no pin selected");
        }
    }

    private void HandleInputMove(Vector3 worldPosition)
    {
        // Update the position of the currently dragged pin (drag mode only)
        if (currentlyDraggedPin != null && currentlyDraggedPin.isFollowing)
        {
            currentlyDraggedPin.transform.position = worldPosition;
        }
    }

    private void HandleInputEnd(Vector3 worldPosition)
    {
        Debug.Log($"Input ended at world position: {worldPosition}");
        
        // Stop dragging the current pin if we were dragging
        if (currentlyDraggedPin != null && currentlyDraggedPin.isFollowing)
        {
            StopFollowingPin(currentlyDraggedPin);
            currentlyDraggedPin = null;
        }
    }

    private void SelectPin(PinLogic pin)
    {
        selectedPin = pin;
        Debug.Log($"Selected pin at position: {pin.transform.position}");
    }

    private void DeselectPin(PinLogic pin)
    {
        if (pin == null) return;
        
        selectedPin = null;
        Debug.Log($"Deselected pin");
    }

    private void MovePinToPosition(PinLogic pin, Vector3 worldPosition)
    {
        Debug.Log($"Moving selected pin to position: {worldPosition}");
        
        // Find the closest grid point to the target position
        Transform landingPoint = pin.gridManager.GetClosestPoint(worldPosition);
        
        // Move the pin to the grid point
        pin.transform.parent = landingPoint;
        pin.transform.localPosition = Vector3.zero;
        
        // Update the rubber band
        GeometricRubberBand.Instance.UpdateMovingPin(pin.transform, GeometricRubberBand.MovingPinStatus.NotMoving);
    }

    private void StartFollowingPin(PinLogic pin)
    {
        pin.isFollowing = true;
        pin.transform.parent = null;
        GeometricRubberBand.Instance.UpdateMovingPin(pin.transform, GeometricRubberBand.MovingPinStatus.Moving);
    }

    private void StopFollowingPin(PinLogic pin)
    {
        // Find the closest point on the grid and go to it
        Transform landingPoint = pin.gridManager.GetClosestPoint(pin.transform.position);
        pin.transform.parent = landingPoint;
        pin.transform.localPosition = Vector3.zero;

        pin.isFollowing = false;
        // Update the band that it's no longer moving
        GeometricRubberBand.Instance.UpdateMovingPin(pin.transform, GeometricRubberBand.MovingPinStatus.NotMoving);
    }

    private PinLogic FindClosestPin(Vector3 worldPosition)
    {
        PinLogic[] allPins = FindObjectsOfType<PinLogic>();
        PinLogic closestPin = null;
        float closestDistance = float.MaxValue;
        float maxClickDistance = 1f; // Adjust this value as needed

        foreach (PinLogic pin in allPins)
        {
            float distance = Vector3.Distance(pin.transform.position, worldPosition);
            if (distance < closestDistance && distance <= maxClickDistance)
            {
                closestDistance = distance;
                closestPin = pin;
            }
        }

        return closestPin;
    }
    
    
}