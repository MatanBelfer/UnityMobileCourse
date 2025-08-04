using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    private SettingsItem[] settingsItems;
    
    [Header("References")]
    [SerializeField] private MainMenuCallbacks mainMenu;

    private void Start()
    {
        settingsItems = GetComponentsInChildren<SettingsItem>();

        foreach (var item in settingsItems)
        {
            item.settingsMenu = this;
        }
        
        LoadSettings();
        
        if (mainMenu != null) mainMenu.OnStartGame += CloseMenu;
        
        AfterSaveSettings = null;
        
        AnalyticsManager analyticsManager = ManagersLoader.Analytics;
        if (analyticsManager)
        {
            AfterSaveSettings += analyticsManager.UpdatePermission;
        }
        
        AfterSaveSettings += ManagersLoader.Audio.LoadAudioSettings;

        SceneManager.sceneLoaded += (_,_) => LoadSettings();
    }

    private void LoadSettings()
    {
//        print("Loading settings");
        
        foreach (var item in settingsItems)
        {
            string prefsName = item.playerPrefsName;
            //print(prefsName);
            if (!PlayerPrefs.HasKey(prefsName)) continue;
            //print($"{prefsName} exists");
            
            if (item.uiInputMethod == UIInputMethod.Slider && item.saveAsInt)
            {
                item.SetValue((float)PlayerPrefs.GetInt(prefsName));
            }
            else if (item.uiInputMethod == UIInputMethod.Slider)
            {
  //              float value = PlayerPrefs.GetFloat(prefsName);
//                print($"setting value {value} from {prefsName}");
                item.SetValueFloat(PlayerPrefs.GetFloat(prefsName));
            }
            else if (item.uiInputMethod == UIInputMethod.Toggle)
            {
                item.SetValueBool(PlayerPrefs.GetString(prefsName) == "True");
            }
        }
    }
    
    public void CloseMenu()
    {
        SaveSettings();
        ManagersLoader.Input.LoadControlScheme();
        gameObject.SetActive(false);
    }

    public event Action AfterSaveSettings;
    public void SaveSettings()
    {
        foreach (var item in settingsItems)
        {
            string prefsName = item.playerPrefsName;
            if (item.uiInputMethod == UIInputMethod.Slider && item.saveAsInt)
            {
                int value = item.GetValue<int>();
                PlayerPrefs.SetInt(prefsName,value);
            }
            else if (item.uiInputMethod == UIInputMethod.Slider)
            {
                float value = item.GetValue<float>();
                PlayerPrefs.SetFloat(prefsName, value);
//                print($"Saved {prefsName} as {value}");
            }
            else if (item.uiInputMethod == UIInputMethod.Toggle)
            {
                bool value = item.GetValue<bool>();
                PlayerPrefs.SetString(prefsName, value ? "True" : "False");
            }
        }
        
        AfterSaveSettings?.Invoke();
    }
}