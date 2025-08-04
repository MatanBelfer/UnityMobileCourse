using UnityEngine;
using UnityEngine.UI;

public class PlayClickSoundOnClick : MonoBehaviour
{
    [SerializeField] private Button button;
    void Start()
    {
        button.onClick.AddListener(() => ManagersLoader.Audio.PlaySFX("button_click"));
    }
}
