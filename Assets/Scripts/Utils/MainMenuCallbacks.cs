using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MainMenuCallbacks : MonoBehaviour
{
    public event Action OnStartGame;
    
    public void StartGame()
    {
        OnStartGame?.Invoke();
        SceneManager.LoadScene("Game");

    }
}
