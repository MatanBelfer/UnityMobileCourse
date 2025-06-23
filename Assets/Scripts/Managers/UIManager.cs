using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class UIManager : BaseManager
{
    [InspectorLabel("Pause Menu")] [SerializeField]
    private GameObject pauseMenu;
    [InspectorLabel("Settings Menu")] [SerializeField]
    private GameObject settingsMenu;
    [InspectorLabel("HUD")] [SerializeField]
    private GameObject HUD;
    [InspectorLabel("Screenshot Camera")] [SerializeField]
    private Camera screenshotCamera;
    
    public bool isPauseMenuOpen { get; private set; }
    
    //Singleton for legacy compatibility
    public static UIManager Instance;
    
   
    public void Awake()
    {
       base.Awake();
       Instance = this; // Set for legacy compatibility
    }

    protected override void OnInitialize()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
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
        if (Instance == this)
        {
            Instance = null;
        }
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