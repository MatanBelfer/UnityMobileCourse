using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
	//Score = rawScore - scoreOffset
	[InspectorLabel("Score")]
	private int rawScore = 0; // the score (height) reported by the pins
	private int scoreOffset; // the starting initial score given by the highest pin on start.
	public int highScore { get; private set; }
	private string scorePath = "/score.json";
	
	[InspectorLabel("Pause")]
	public bool isPaused { get; private set; }
	private UIManager uiManager;
	
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
		
		//OnRestartLevel initialization
		OnRestartLevel += SaveHighScoreToFile;
		OnRestartLevel += () => rawScore = 0;
		
		//Load the high score from file
		string path = Application.persistentDataPath + scorePath;
		if (System.IO.File.Exists(path))
		{
			string json = System.IO.File.ReadAllText(path);
			ScoreData data = JsonUtility.FromJson<ScoreData>(json);
			highScore = data.score;
		}
	}

	public void Start()
	{
		uiManager = UIManager.Instance;
	}
	
	//test pause 
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			SetPause(!isPaused);
			print($"{isPaused : True/False}");
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
		OnRestartLevel?.Invoke();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	
	private void SaveHighScoreToFile()
	{
		if (!(GetScore() > highScore)) return;
		highScore = GetScore();
		string json = JsonUtility.ToJson(new ScoreData(highScore));
		string path = Application.persistentDataPath + scorePath;
		System.IO.File.WriteAllText(path, json);
		print($"saved score to {path}");
	}

	[Serializable]
	private class ScoreData
	{
		public int score;
		public ScoreData(int score) => this.score = score;
	}

	public void SetInitialScore(int initialHeight)
	{
		if (initialHeight > scoreOffset) scoreOffset = initialHeight;
	}

	public void UpdateScore(int landingRow)
	{
		//calculates the new score given the row the pin landed on
		if (landingRow > rawScore) rawScore = landingRow;
	}

	public int GetScore()
	{
		return math.clamp(rawScore - scoreOffset,0,int.MaxValue);
	}
	
	public void SetPause(bool pauseState)
	{
		if (isPaused == pauseState) return;
		isPaused = pauseState;
		Time.timeScale = isPaused ? 0 : 1;
		uiManager.PauseMenu(isPaused);
	}
}
