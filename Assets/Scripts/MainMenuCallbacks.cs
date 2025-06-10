using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuCallbacks : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
