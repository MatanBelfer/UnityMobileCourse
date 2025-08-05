using TMPro;
using UnityEngine;

public class GetHighScoreFromGameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    void Start()
    {
        text.text = $"High Score: {ManagersLoader.Game.highScore}";
    }
}
