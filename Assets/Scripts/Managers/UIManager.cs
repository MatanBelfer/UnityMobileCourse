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
    [InspectorLabel("Screenshot Camera")] [SerializeField]
    private Camera screenshotCamera;
    
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