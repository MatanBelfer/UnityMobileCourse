using UnityEngine;
using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Unity.Mathematics;


public enum SettingType
{
    NA,
    ControlScheme,
    Difficulty,
    DataCollectionPermission
}

public enum UIInputMethod
{
    Slider,
    Toggle
}

public enum DataCollectionPermission
{
    Allowed,
    NotAllowed
}

public class SettingsItem : MonoBehaviour
{
    //component for settings menu items 
    [Header("Attributes")]
    [SerializeField] private string _playerPrefsName;
    public string playerPrefsName => _playerPrefsName;
    [SerializeField] [Tooltip("If you want the setting to refer to an enum, set this to the enum's name." +
                              "In this case, it will be saved in playerPrefs as int. Choose NA otherwise")]
    public SettingType settingType;
    public bool saveAsInt => settingType != SettingType.NA;
    
    
    [Header("Heirarchy")]
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private GameObject uiInputObject; //slider or toggle
    private MonoBehaviour inputComponent; // the actual component reference        
    public UIInputMethod uiInputMethod {private set; get;}

    public SettingsMenu settingsMenu;
    

    public void Awake()
    {
        //set uiInputMethod
        
        inputComponent = (MonoBehaviour)uiInputObject.GetComponent(typeof(Slider));
        if (inputComponent is Slider slider)
        {
            uiInputMethod = UIInputMethod.Slider;
            slider.onValueChanged.AddListener(SetValueFloat);
            slider.onValueChanged.AddListener(_ => StartCoroutine(SaveSettingsNextFrame()));
            return;
        }
        inputComponent = (MonoBehaviour)uiInputObject.GetComponent(typeof(Toggle));
        if (inputComponent is Toggle toggle)
        {
            uiInputMethod = UIInputMethod.Toggle;
            toggle.onValueChanged.AddListener(SetValueBool);
            toggle.onValueChanged.AddListener(_ => StartCoroutine(SaveSettingsNextFrame()));
        }
        else
        {
            Debug.LogError("UIInputObject must be a slider or toggle");
        }
    }

    /// <summary>
    /// Waits one frame then orders SettingsMenu to save settings
    /// </summary>
    private IEnumerator SaveSettingsNextFrame()
    {
        yield return null;
        settingsMenu.SaveSettings();
    }

    public string GetValueName<T>(T value) where T : struct
    {
        //if the setting has a text that shows it's value (ex. ControlScheme)
        switch (settingType)
        {
            case SettingType.NA:
                return value.ToString();
            case SettingType.ControlScheme:
                if (value is float f)
                {
                    ControlScheme scheme = (ControlScheme)(int)math.round(f);
                    return scheme.GetName();
                }
                throw new Exception($"ControlScheme value is not a float: {value}");
            case SettingType.Difficulty:
                if (value is float f2)
                {
                    Difficulty difficulty = (Difficulty)(int)math.round(f2);
                    return difficulty.GetName();
                }
                throw new Exception($"Difficulty value is not a float: {value}");
            case SettingType.DataCollectionPermission:
                if (value is float f3)
                {
                    DataCollectionPermission permission = (DataCollectionPermission)(int)math.round(f3);
                    return permission.GetName();
                }
                throw new Exception($"DataCollectionPermission value is not a float: {value}");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetValueFloat(float value)
    {
//        print($"Set value triggered for {playerPrefsName}");
        SetValue<float>(value);
    }

    public void SetValueBool(bool value)
    {
        SetValue<bool>(value);
    }

    public void SetValue<T>(T value) where T : struct
    {
        //update value text
        if (valueText)
        {
            valueText.text = GetValueName(value);
        }
        //set the value in the slider or toggle
        //print($"SetValue got {typeof(T)}");
        SetValueInUIObject<T>(value);
    }

    private void SetValueInUIObject<T>(T value) where T : struct
    {
        if (uiInputMethod == UIInputMethod.Slider && typeof(T) == typeof(float))
        {
            (inputComponent as Slider).value = (float)(object)value;
        }
        else if (uiInputMethod == UIInputMethod.Toggle && typeof(T) == typeof(bool))
        {
            (inputComponent as Toggle).isOn = (bool)(object)value;
        }
        else
        {
            throw new Exception(
                $"Tried to set wrong value type ({typeof(T)}) to UI component ({nameof(uiInputMethod)}).");
        }
    }

    public T GetValue<T>() where T : struct
    {
        if (typeof(T) == typeof(float) && uiInputMethod == UIInputMethod.Slider)
        {
            return (T)(object)(inputComponent as Slider).value;
        }
        if (typeof(T) == typeof(string) && uiInputMethod == UIInputMethod.Toggle)
        {
            return (T)(object)((inputComponent as Toggle).isOn ? "True" : "False");
        }
        if (typeof(T) == typeof(int) && uiInputMethod == UIInputMethod.Slider)
        {
            return (T)Convert.ChangeType(math.round(uiInputObject.GetComponent<Slider>().value),typeof(T));
        }
        
        throw new Exception($"GetValue was called with type {typeof(T)} for UIInputMethod {uiInputMethod}, " +
                            $"but only the following calls are supported:\n" +
                            $"GetValue<float> when uiInputMethod = Slider\n" +
                            $"GetValue<int> when uiInputMethod = Slider\n" +
                            $"GetValue<string> when uiInputMethod = Toggle");
    }
}
