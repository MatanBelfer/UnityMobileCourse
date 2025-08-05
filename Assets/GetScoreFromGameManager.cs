using TMPro;
using UnityEngine;

public class GetScoreFromGameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private void Update()
    {
        text.text = $"Score: {ManagersLoader.Game.currentScore}";
    }
}
