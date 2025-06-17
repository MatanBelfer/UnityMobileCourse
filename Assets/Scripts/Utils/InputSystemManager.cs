using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RubberClimber
{
    public class InputSystemManager : MonoBehaviour
    {
        public PlayerInputActions InputActions;

        public static InputSystemManager Instance { get; private set; }

        private ControlScheme InputMode = ControlScheme.DragAndDrop;

        [Header("Drag Mode Settings")] [SerializeField]
        private float maxClickDistance = 0.5f; // Max distance to detect pin clicks

        private PinLogic CurrentPin = null;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InputActions = new PlayerInputActions();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            InputActions.PinMovement.Enable();
            InputActions.PinMovement.Click.started += OnClickStarted;
            InputActions.PinMovement.Click.canceled += OnClickEnded;
            InputActions.PinMovement.Position.performed += OnPositionChanged;
            InputActions.PinMovement.ToggleMode.started += OnToggleModePressed;
        }

        private void OnDisable()
        {
            InputActions.PinMovement.Click.started -= OnClickStarted;
            InputActions.PinMovement.Click.canceled -= OnClickEnded;
            InputActions.PinMovement.Position.performed -= OnPositionChanged;
            InputActions.PinMovement.ToggleMode.started -= OnToggleModePressed;
            InputActions.PinMovement.Disable();
        }

        private void OnDestroy()
        {
            InputActions.Dispose();
        }

        private void OnClickStarted(InputAction.CallbackContext context)
        {
            Vector3 clickPosition = ScreenToWorldPosition(InputActions.PinMovement.Position.ReadValue<Vector2>());

            PinLogic tmpPin = FindClosestPin(clickPosition);
            if (tmpPin != null)
            {
                if (InputMode == ControlScheme.TapTap)
                {
                    //select mode
                }
                else
                {
                    //drag mode
                    CurrentPin = tmpPin;
                    Debug.Log("drag mode clicked");
                    CurrentPin.StartFollowingPin(CurrentPin);
                }
            }
            else
            {
                Debug.Log("Clicked on nothing");
            }
        }

        private void OnClickEnded(InputAction.CallbackContext context)
        {
            Vector3 endPosition = ScreenToWorldPosition(InputActions.PinMovement.Position.ReadValue<Vector2>());
            if (InputMode == ControlScheme.TapTap)
            {
                //select mode
                PinLogic tmpPin = FindClosestPin(endPosition);
                if (tmpPin == null && CurrentPin != null)
                {
                    CurrentPin.MovePinToPosition(CurrentPin, endPosition);
                    CurrentPin.StopFollowingPin(CurrentPin);
                    CurrentPin = null;
                }
                else
                {
                    if (CurrentPin != null && CurrentPin == tmpPin)
                    {
                        CurrentPin = null; //reset selection
                    }
                    else
                    {
                        CurrentPin = tmpPin; // selected another pin 
                    }
                }
            }
            else
            {
                //drag mode


                CurrentPin.MovePinToPosition(CurrentPin, endPosition);
                CurrentPin.StopFollowingPin(CurrentPin);
                CurrentPin = null;
            }
        }


        private void OnPositionChanged(InputAction.CallbackContext context)
        {
            if (CurrentPin != null && CurrentPin.isFollowing &&
                InputMode == ControlScheme.DragAndDrop) // Only in drag mode
            {
                Vector2 screenPosition = context.ReadValue<Vector2>();
                Vector3 worldPosition = ScreenToWorldPosition(screenPosition);
                CurrentPin.transform.position = worldPosition;
            }
        }

        private void OnToggleModePressed(InputAction.CallbackContext context)
        {
            SetInputMode(InputMode == ControlScheme.DragAndDrop
                ? ControlScheme.TapTap
                : ControlScheme.DragAndDrop);
        }

        public void SetInputMode(ControlScheme newMode)
        {
            if (newMode == InputMode) return;

            InputMode = newMode;

            // Cancel any current dragging when switching modes
            if (CurrentPin != null)
            {
                if (CurrentPin.isFollowing)
                {
                    CurrentPin.StopFollowingPin(CurrentPin);
                }

                CurrentPin = null;
            }

            Debug.Log($"Switched to {InputMode}");
        }

        private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        {
            if (Camera.main == null)
            {
                Debug.LogWarning("No main camera found!");
                return Vector3.zero;
            }

            Vector3 worldPosition =
                Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y,
                    Camera.main.nearClipPlane));
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
                if (distance < closestDistance && distance <= maxClickDistance)
                {
                    closestDistance = distance;
                    closestPin = pin;
                }
            }

            return closestPin;
        }
    }
}