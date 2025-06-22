using UnityEngine;
using UnityEngine.InputSystem;

//TODO class wide refactor 
//move pin and other game logic to another class
//leave only input handlers and event calls

namespace RubberClimber
{
    public enum ControlScheme
    {
        DragAndDrop = 0,
        TapTap = 1 //tap pin to select, tap a place to make it move there
    }

    public class InputSystemManager : MonoBehaviour
    {
        private PlayerInputActions _inputActions;
        public static InputSystemManager Instance { get; private set; }


        [Header("Input Settings")] [SerializeField]
        private ControlScheme controlScheme = ControlScheme.DragAndDrop;

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

            controlScheme = (ControlScheme)PlayerPrefs.GetInt("controlScheme");
        }

        private void OnEnable()
        {
            _inputActions.PinMovement.Enable();

            //UI related 
            _inputActions.PinMovement.TakeScreenshot.performed += OnTakeScreenshot;


            // _inputActions.PinMovement.Click.started += OnClickStarted;
            _inputActions.PinMovement.Click.canceled += OnClickEnded;
            _inputActions.PinMovement.Position.started += OnPositionChanged;
            _inputActions.PinMovement.Position.performed += OnPositionChanged;
            _inputActions.PinMovement.ToggleMode.started += OnToggleModePressed;
        }

        private void OnTakeScreenshot(InputAction.CallbackContext obj)
        {
            StartCoroutine(UIManager.Instance.TakeScreenshot());
        }


        private void OnDisable()
        {
            if (_inputActions != null)
            {
                // _inputActions.PinMovement.Click.started -= OnClickStarted;
                _inputActions.PinMovement.Click.canceled -= OnClickEnded;
                _inputActions.PinMovement.Position.started -= OnPositionChanged;
                _inputActions.PinMovement.Position.performed -= OnPositionChanged;
                _inputActions.PinMovement.ToggleMode.started -= OnToggleModePressed;
                _inputActions.PinMovement.Disable();
            }
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }

        private void OnClickEnded(InputAction.CallbackContext context)
        {
            Vector3 endPosition = GetClickWorldPosition();

            if (controlScheme == ControlScheme.DragAndDrop)
            {
                HandleDragEnd(endPosition);
            }
            else
            {
                HandleSelectEnd(endPosition);
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

            if (clickedPin == null && _currentPin != null)
            {
                // Move selected pin to empty space with animation
                _currentPin.MovePinToPosition(_currentPin, endPosition, true); // With animation for select
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
}