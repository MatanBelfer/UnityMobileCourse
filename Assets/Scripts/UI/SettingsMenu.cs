using System;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace RubberClimber
{
    public class SettingsMenu : MonoBehaviour
    {
        //Functionality of the settings menu
        private float musicVol;
        private float masterVol;
        private ControlScheme controlScheme;
        private InputSystemManager inputSystemManager;

        [Header("Sliders")]
        [SerializeField] private Slider musicVolSlider;
        [SerializeField] private Slider masterVolSlider; 
        [SerializeField] private Slider controlSchemeSlider;
        [SerializeField] private TMP_Text controlSchemeText;
        
        [Header("MainMenu")]
        [SerializeField] private MainMenuCallbacks mainMenu;

        private void Start()
        {
            musicVolSlider.onValueChanged.AddListener(SetMusicVolume);
            masterVolSlider.onValueChanged.AddListener(SetSFXVolume);
            controlSchemeSlider.onValueChanged.AddListener(SetControlScheme);
            
            inputSystemManager = InputSystemManager.Instance;
            
            LoadSettings();
            SetAllSettings();
            
            if (mainMenu != null) mainMenu.OnStartGame += CloseMenu;
        }

        private void LoadSettings()
        {
            musicVol = PlayerPrefs.GetFloat("musicVol");
            masterVol = PlayerPrefs.GetFloat("masterVol");
            controlScheme = (ControlScheme)PlayerPrefs.GetInt("controlScheme");
            //set sliders
            musicVolSlider.value = musicVol;
            masterVolSlider.value = masterVol;
            controlSchemeSlider.value = (float)controlScheme;
            controlSchemeText.text = controlScheme.ToString();
        }

        private void SetAllSettings()
        {
            SetMusicVolume(musicVol);
            SetSFXVolume(masterVol);
            SetControlScheme(controlScheme);
        }

        public void SetMusicVolume(float volume)
        {
            musicVol = volume;
        }
        
        public void SetSFXVolume(float volume)
        {
            masterVol = volume;
        }

        public void SetControlScheme(float value)
        {
            if (value % 1 != 0) Debug.LogError("Control scheme value must be an integer");
            controlScheme = (ControlScheme)value;
            SetControlScheme(controlScheme);
        }

        private void SetControlScheme(ControlScheme scheme)
        {
            controlSchemeText.text = controlScheme.ToString();
            inputSystemManager?.SetControlScheme(scheme);
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
            PlayerPrefs.SetFloat("masterVol", masterVol);
            PlayerPrefs.SetInt("controlScheme", (int)controlScheme);
            print("Settings Saved");
        }
    }
}