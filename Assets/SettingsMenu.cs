using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    //Functionality of the settings menu
    private float musicVol;
    private float sfxVol;
    private ControlScheme controlScheme;
    public enum ControlScheme
    {
        DragAndDrop = 0,
        TapTap = 1 //tap pin to select, tap place to make it move there
    }

    [Header("Sliders")]
    [SerializeField] private Slider musicVolSlider;
    [SerializeField] private Slider sfxVolSlider; 
    [SerializeField] private Slider controlSchemeSlider;
    [SerializeField] private TMP_Text controlSchemeText;

    private void Start()
    {
        musicVolSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolSlider.onValueChanged.AddListener(SetSFXVolume);
        controlSchemeSlider.onValueChanged.AddListener(SetControlScheme);
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        musicVol = PlayerPrefs.GetFloat("musicVol");
        sfxVol = PlayerPrefs.GetFloat("sfxVol");
        controlScheme = (ControlScheme)PlayerPrefs.GetInt("controlScheme");
        //set sliders
        musicVolSlider.value = musicVol;
        sfxVolSlider.value = sfxVol;
        controlSchemeSlider.value = (float)controlScheme;
        controlSchemeText.text = GetControlSchemeName(controlScheme);
    }

    public void SetMusicVolume(float volume)
    {
        musicVol = volume;
        print($"music volume: {musicVol}");
    }
    public void SetSFXVolume(float volume)
    {
        sfxVol = volume;
    }

    public void SetControlScheme(float value)
    {
        if (value % 1 != 0) Debug.LogError("Control scheme value must be an integer");
        controlScheme = (ControlScheme)value;
        controlSchemeText.text = GetControlSchemeName(controlScheme);
    }

    public string GetControlSchemeName(ControlScheme scheme)
    {
        switch (scheme)
        {
            case ControlScheme.DragAndDrop:
                return "Drag and Drop";
            case ControlScheme.TapTap:
                return "Tap Tap";
            default:
                return "Unknown";
        }
    }

    public void CloseMenu()
    {
        ApplySettings();
        SaveSettings();
        gameObject.SetActive(false);
    }
    
    private void ApplySettings()
    {
        //Apply settings
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("musicVol", musicVol);
        PlayerPrefs.SetFloat("sfxVol", sfxVol);
        PlayerPrefs.SetInt("controlScheme", (int)controlScheme);
    }
}
