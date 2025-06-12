using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    //Singleton
	public static GameManager Instance;
	private int rawScore = 0; // the score (height) reported by the pins
	private int scoreOffset; // the starting initial score given by the highest pin on start. 
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
		OnRestartLevel += SaveScoreToFile;
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

	private void SaveScoreToFile()
	{
		string json = JsonUtility.ToJson(new ScoreData(GetScore()));
		System.IO.File.WriteAllText(Application.persistentDataPath + "/score.json", json);
		print($"saved score to {Application.persistentDataPath}/score");
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
		//testing
		print(GetScore());
	}

	public int GetScore()
	{
		return math.clamp(rawScore - scoreOffset,0,int.MaxValue);
	}
}
