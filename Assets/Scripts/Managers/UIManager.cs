using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [InspectorLabel("Pause Menu")] [SerializeField]
    private GameObject pauseMenu;

    [InspectorLabel("Settings Menu")] [SerializeField]
    private GameObject settingsMenu;

    [InspectorLabel("HUD")] [SerializeField]
    private GameObject HUD;

    public bool isPauseMenuOpen { get; private set; }
    public GameManager gameManager;

    //Singleton
    public static UIManager Instance;

    //Initialize the singleton
    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Start()
    {
        gameManager = GameManager.Instance;
        pauseMenu.SetActive(false);
    }

    public void PauseMenu(bool openState)
    {
        pauseMenu.SetActive(openState);
    }

    public IEnumerator TakeScreenshot()
    {
        bool wasHUDActive = HUD.activeSelf;
        bool wasPauseMenuActive = pauseMenu.activeSelf;

        HideAll();

        yield return new WaitForEndOfFrame();

        var path = Application.dataPath + "/Screenshots/Screenshot_" +
                   System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";
        ScreenCapture.CaptureScreenshot(path, 1);
        Debug.Log($"Added new screenshot {path}");

        yield return new WaitForEndOfFrame();

        HUD.SetActive(wasHUDActive);
        pauseMenu.SetActive(wasPauseMenuActive);
    }

    private void HideAll()
    {
        Debug.Log($"Hiding all UI \r\n {HUD.name} \r\n {pauseMenu.name}");
        
        HUD.SetActive(false);
        pauseMenu.SetActive(false);
        
    }

    private void ShowAll()
    {
        Debug.Log("Showing all UI");
        HUD.SetActive(true);
        pauseMenu.SetActive(true);
    }
}