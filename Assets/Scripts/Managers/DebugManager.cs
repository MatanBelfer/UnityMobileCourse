using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class DebugManager : BaseManager
{
    [Header("Debug UI")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private Button debugButton;
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private TMP_InputField scoreInput;
    [SerializeField] private Slider timeScaleSlider;
    [SerializeField] private Button resetButton;

    [Header("Debug Button Settings")]
    private const int REQUIRED_CLICKS = 5;
    private const float CLICK_TIMEOUT = 3f;
    private int clickCount;
    private float lastClickTime;

    private GameManager gameManager;
    private bool isDebugMenuOpen;
    private PlayerInputActions inputActions;

    protected override void OnInitialize()
    {
        Debug.Log("DebugManager: Initializing...");
        gameManager = ManagersLoader.Game;
        
        // Initialize input actions
        inputActions = new PlayerInputActions();
        inputActions.Enable();
        
        // Ensure debug panel is hidden initially
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }

        // Setup UI listeners
        if (debugButton != null)
        {
            debugButton.onClick.AddListener(OnClickDebugBTN);
            Debug.Log("DebugManager: Debug button listener added");
        }
        else
        {
            Debug.LogError("DebugManager: Debug button reference is missing!");
        }

        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        
        if (scoreInput != null)
            scoreInput.onEndEdit.AddListener(OnScoreChanged);
        
        if (timeScaleSlider != null)
            timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetGame);

        // Initialize click tracking
        clickCount = 0;
        lastClickTime = 0f;
    }

    private void OnResetGame()
    {
        // throw new System.NotImplementedException();
    }

    private void OnTimeScaleChanged(float arg0)
    {
        // throw new System.NotImplementedException();
    }

    private void OnScoreChanged(string arg0)
    {
        // throw new System.NotImplementedException();
    }

    private void OnDifficultyChanged(int arg0)
    {
        // throw new System.NotImplementedException();
    }

    protected override void OnReset()
    {
        // throw new System.NotImplementedException();
    }

    private void Update()
    {
        // Alternative input check using the new Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                debugButton.GetComponent<RectTransform>(),
                Mouse.current.position.ReadValue(),
                Camera.main))
            {
                OnClickDebugBTN();
            }
        }
    }

    public void OnClickDebugBTN()
    {
        Debug.Log($"DebugManager: Debug button clicked. Current count: {clickCount}");
        ManagersLoader.Audio.PlaySFX("button_click");
        
        // Check if we've timed out
        if (Time.time - lastClickTime > CLICK_TIMEOUT)
        {
            Debug.Log("DebugManager: Click timeout - resetting count");
            clickCount = 0;
        }

        // Update click count and time
        clickCount++;
        lastClickTime = Time.time;

        Debug.Log($"DebugManager: Updated click count to {clickCount}");

        // Check if we've reached the required number of clicks
        if (clickCount >= REQUIRED_CLICKS)
        {
            Debug.Log("DebugManager: Required clicks reached - toggling debug menu");
            ToggleDebugMenu();
            clickCount = 0; // Reset click count after activating
        }
    }

    private void ToggleDebugMenu()
    {
        isDebugMenuOpen = !isDebugMenuOpen;
        Debug.Log($"DebugManager: Toggling debug menu - IsOpen: {isDebugMenuOpen}");
        
        if (debugPanel != null)
        {
            debugPanel.SetActive(isDebugMenuOpen);
        }

        // Pause the game when debug menu is open without showing pause menu
        if (gameManager != null)
        {
            // Set the game's time scale directly instead of using SetPause
            Time.timeScale = isDebugMenuOpen ? 0f : 1f;
        }

        // Update UI values when opening
        if (isDebugMenuOpen)
        {
            UpdateDebugUIValues();
        }
    }

    private void UpdateDebugUIValues()
    {
        // throw new System.NotImplementedException();
    }

    protected override void OnCleanup()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
            inputActions = null;
        }

        if (debugButton != null)
            debugButton.onClick.RemoveListener(OnClickDebugBTN);
        
        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyChanged);
        
        if (scoreInput != null)
            scoreInput.onEndEdit.RemoveListener(OnScoreChanged);
        
        if (timeScaleSlider != null)
            timeScaleSlider.onValueChanged.RemoveListener(OnTimeScaleChanged);
        
        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetGame);
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Dispose();
        }
    }
}