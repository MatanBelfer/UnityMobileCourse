using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class DebugManager : BaseManager
{
    [Header("Debug UI")] [SerializeField] private GameObject debugPanel;
    [SerializeField] private Button debugButton;
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private TMP_InputField scoreInput;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button resetButton;
    [SerializeField] private Slider godModeToggle;
    [SerializeField] private TMP_Text GodModeText;

    [Header("Debug Button Settings")] private const int REQUIRED_CLICKS = 5;
    private const float CLICK_TIMEOUT = 3f;
    private int clickCount;
    private float lastClickTime;

    private bool isDebugMenuOpen;
    private bool isGodModeEnabled;
    private PlayerInputActions inputActions;

    protected override void OnInitialize()
    {
        inputActions = new PlayerInputActions();
        inputActions.Enable();

        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }

        // Setup UI listeners
        if (debugButton != null)
        {
            debugButton.onClick.AddListener(OnClickDebugBTN);
        }
        else
        {
            Debug.LogError("DebugManager: Debug button reference is missing!");
        }

        if (difficultyDropdown != null)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);


        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetGame);

        if (godModeToggle != null)
            godModeToggle.onValueChanged.AddListener(OnGodModeToggled);

        clickCount = 0;
        lastClickTime = 0f;
        isGodModeEnabled = false;
    }

    private void OnResetGame()
    {
    }

    private void OnTimeScaleChanged(float arg0)
    {
    }

    private void OnGodModeToggled(float arg0)
    {
        isGodModeEnabled = arg0 > 0.5f;

        if (ManagersLoader.Game != null)
        {
            if (isGodModeEnabled)
            {
                ManagersLoader.Game.EnableGodMode();
                Debug.Log("God Mode Enabled");
            }
            else
            {
                Debug.Log("God Mode Disabled - Implementation needed in GameManager");
            }
        }
    }

    public void OnChangeScore()
    {
        ManagersLoader.Game.AddScore(int.Parse(scoreInput.text));
    }

    private void OnDifficultyChanged(int arg0)
    {
    }

    protected override void OnReset()
    {
        // Add your reset logic here
    }

    private void Update()
    {
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
        GodModeText.text = isGodModeEnabled? "Enabled" : "Disabled";
    }

    public void OnClickDebugBTN()
    {
        ManagersLoader.Audio.PlaySFX("button_click");

        if (Time.time - lastClickTime > CLICK_TIMEOUT)
        {
            clickCount = 0;
        }

        clickCount++;
        lastClickTime = Time.time;


        if (clickCount >= REQUIRED_CLICKS)
        {
            ToggleDebugMenu();
            clickCount = 0;
        }
    }

    private void ToggleDebugMenu()
    {
        isDebugMenuOpen = !isDebugMenuOpen;

        if (debugPanel != null)
        {
            debugPanel.SetActive(isDebugMenuOpen);
        }

        if (ManagersLoader.Game != null)
        {
            Time.timeScale = isDebugMenuOpen ? 0f : 1f;
        }

        if (isDebugMenuOpen)
        {
            UpdateDebugUIValues();
        }
    }

    private void UpdateDebugUIValues()
    {
        scoreText.text = ManagersLoader.Game.GetScore().ToString();

        if (godModeToggle != null)
        {
            godModeToggle.SetValueWithoutNotify(isGodModeEnabled ? 1f : 0f);
        }

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

        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetGame);

        if (godModeToggle != null)
            godModeToggle.onValueChanged.RemoveListener(OnGodModeToggled);
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Dispose();
        }
    }
}