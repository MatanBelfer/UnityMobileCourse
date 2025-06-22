using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using Unity.Mathematics;

namespace RubberClimber
{
    public enum UIInputMethod
    {
        Slider,
        Toggle
    }

    public enum SettingType
    {
        NA,
        ControlScheme,
        Difficulty
    }
    
    public class SettingsItem : MonoBehaviour
    {
        //component for settings menu items 
        [Header("Attributes")]
        [SerializeField] private string _playerPrefsName;
        [SerializeField] [Tooltip("If you want the setting to refer to an enum, set this to the enum's name." +
                                  "In this case, it will be saved in playerPrefs as int. Choose NA otherwise")]
        public SettingType settingType;
        [Header("Children")]
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private GameObject uiInputObject; //slider or toggle
        
        public string playerPrefsName => _playerPrefsName;
        public UIInputMethod uiInputMethod {private set; get;}
        public bool saveAsInt => settingType != SettingType.NA;
        private Action<object> SetUIObjectValue; // sets the value of the slider or toggle 

        public void Awake()
        {
            // InitializeGetValueName();
            
            //set uiInputMethod
            Slider slider = uiInputObject.GetComponent<Slider>();
            Toggle toggle = uiInputObject.GetComponent<Toggle>();
            if (slider)
            {
                uiInputMethod = UIInputMethod.Slider;
                SetUIObjectValue = (value) =>
                {
                    if (value is float f) slider.value = f;
                };
                slider.onValueChanged.AddListener(SetValueFloat);
            }
            else if (toggle)
            {
                uiInputMethod = UIInputMethod.Toggle;
                SetUIObjectValue = (value) =>
                {
                    if (value is bool b) toggle.isOn = b;
                };
                toggle.onValueChanged.AddListener(SetValueBool);
            }
            else
            {
                Debug.LogError("UIInputObject must be a slider or toggle");
            }
        }

        public string GetValueName<T>(T value) where T : struct
        {
            //if the setting has a text that shows it's value (ex. ControlScheme)
            switch (settingType)
            {
                case SettingType.NA:
                    return value.ToString();
                case SettingType.ControlScheme:
                    if (value is float f) return ((ControlScheme)(int)math.round(f)).ToString();
                    throw new Exception($"ControlScheme value is not a float: {value}");
                case SettingType.Difficulty:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        // private void InitializeGetValueName()
        // {
        //     //set the GetValueName function
        //     switch (settingType)
        //     {
        //         case SettingType.NA:
        //             GetValueName = v => v.ToString();
        //             break;
        //         case SettingType.ControlScheme:
        //             GetValueName = v =>
        //             {
        //                 if (v is float f) return ((ControlScheme)(int)math.round(f)).ToString();
        //                 throw new Exception($"ControlScheme value is not an int: {v}");
        //             };
        //             break;
        //         case SettingType.Difficulty:
        //             throw new NotImplementedException();
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        //     
        //     // if (settingType == SettingType.NA || settingType.Length == 0)
        //     // {
        //     //     GetValueName = v => v.ToString();
        //     // }
        //     // else
        //     // {
        //     //     try
        //     //     {
        //     //         Type enumType = Type.GetType(settingType, true,  true);
        //     //         GetValueName = v => Convert.ChangeType((int)v, enumType).ToString();
        //     //         settingTypeIsValidEnum = true;
        //     //     }
        //     //     catch (TypeLoadException e)
        //     //     {
        //     //         Debug.LogWarning($"Possible typo in settingType: {settingType} .");
        //     //     }
        //     //     catch (Exception e)
        //     //     {
        //     //         Debug.LogWarning(e);
        //     //     }
        //     // }
        // }

        public void SetValueFloat(float value)
        {
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
            print($"SetValue got {typeof(T)}");
            SetValueInUIObject<T>(value);
        }

        private void SetValueInUIObject<T>(T value) where T : struct
        {
            print($"SetValueInUIObject got {typeof(T)}");
            if (uiInputMethod == UIInputMethod.Slider && typeof(T) == typeof(float))
            {
                print($"entered slider with value {(float)(object)value}. {value}");
                uiInputObject.GetComponent<Slider>().value = (float)(object)value;
            }
            else if (uiInputMethod == UIInputMethod.Toggle && typeof(T) == typeof(bool))
            {
                print("entered toggle");
                uiInputObject.GetComponent<Toggle>().isOn = (bool)(object)value;
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
                return (T)(object)uiInputObject.GetComponent<Slider>().value;
            }
            if (typeof(T) == typeof(string) && uiInputMethod == UIInputMethod.Toggle)
            {
                return (T)(object)(uiInputObject.GetComponent<Toggle>().isOn ? "True" : "False");
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
}