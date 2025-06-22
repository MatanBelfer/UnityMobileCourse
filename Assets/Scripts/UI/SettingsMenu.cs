using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    private SettingsItem[] settingsItems;
    
    [Header("References")]
    [SerializeField] private MainMenuCallbacks mainMenu;
    [SerializeField] private InputSystemManager inputManager;

    private void Start()
    {
        settingsItems = GetComponentsInChildren<SettingsItem>();
        
        LoadSettings();
        
        if (mainMenu != null) mainMenu.OnStartGame += CloseMenu;
        
        inputManager = InputSystemManager.Instance;

        SceneManager.sceneLoaded += (_,_) => LoadSettings();
    }

    private void LoadSettings()
    {
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
                //print($"setting from {prefsName}");
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
        if (inputManager)
        {
            inputManager.LoadControlScheme();
        }
        gameObject.SetActive(false);
    }

    private void SaveSettings()
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
            }
            else if (item.uiInputMethod == UIInputMethod.Toggle)
            {
                bool value = item.GetValue<bool>();
                PlayerPrefs.SetString(prefsName, value ? "True" : "False");
            }
        }
    }
}