using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    //Singleton
	public static GameManager Instance;
	public event Action OnRestartLevel;

	//Initialize the singleton
	public void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		else
		{
			Instance = this;
		}
	}
	
	//Destroy the singleton
	public void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public void RestartLevel()
	{
		OnRestartLevel += () => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		OnRestartLevel?.Invoke();
	}
}
