using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : BaseManager
{
    [InspectorLabel("Pause Menu")] [SerializeField]
    private GameObject pauseMenu;
    [InspectorLabel("Settings Menu")] [SerializeField]
    private GameObject settingsMenu;
    [InspectorLabel("HUD")] [SerializeField]
    private GameObject HUD;
    [InspectorLabel("Score Text Object")] [SerializeField]
    private TMP_Text scoreText;
    [InspectorLabel("Screenshot Camera")] [SerializeField]
    private Camera screenshotCamera;
    
    public bool isPauseMenuOpen { get; private set; }

    
   
    public void Awake()
    {
       base.Awake();
    }

    protected override void OnInitialize()
    {
        //Initialize element visibility
        pauseMenu?.SetActive(false);
        HUD?.SetActive(false);
        
        //react to score change
        GameManager gameManager = ManagersLoader.Game;
        gameManager.OnScoreChanged += score => scoreText.text = $"Score: {score}";
        scoreText.text = "Score: 0";
        
        //react to start game
        SceneManager.sceneLoaded += (scene, _) =>
        {
            if (scene.name == "Game") HUD?.SetActive(true);
        };
    }

    protected override void OnReset()
    {
        // Reset UI state
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        isPauseMenuOpen = false;
    }

    protected override void OnCleanup()
    {
        // Reset UI state when cleaning up
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        isPauseMenuOpen = false;
    }

    private void OnDestroy()
    {
   
    }
    
    public void PauseMenu(bool openState)
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(openState);
            isPauseMenuOpen = openState;
        }
    }
    
    public IEnumerator TakeScreenshot()
    {
        if (screenshotCamera == null)
        {
            Debug.LogError("Screenshot camera is not assigned!");
            yield break;
        }

        yield return new WaitForEndOfFrame();

        // Create a RenderTexture to capture the camera output
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        screenshotCamera.targetTexture = renderTexture;
        
        // Render the camera
        screenshotCamera.Render();
        
        // Read the pixels from the RenderTexture
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();
        
        // Reset camera and RenderTexture
        screenshotCamera.targetTexture = null;
        RenderTexture.active = null;
        
        // Save the screenshot
        byte[] data = screenshot.EncodeToPNG();
        var path = Application.dataPath + "/Screenshots/Screenshot_" +
                   System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";
        
        // Ensure directory exists
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
        System.IO.File.WriteAllBytes(path, data);
        
        Debug.Log($"Added new screenshot {path}");
        
        // Clean up
        Destroy(screenshot);
        Destroy(renderTexture);
    }
    
    private void HideAll()
    {
        Debug.Log($"Hiding all UI \r\n {HUD.name} \r\n {pauseMenu.name}");
        
        if (HUD != null) HUD.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
    }
    
    private void ShowAll()
    {
        Debug.Log("Showing all UI");
        if (HUD != null) HUD.SetActive(true);
        if (pauseMenu != null) pauseMenu.SetActive(true);
    }
}