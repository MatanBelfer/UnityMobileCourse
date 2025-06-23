using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;

public enum Difficulty
{
    Easy,
    Hard
}

public class GameManager : BaseManager
{
    //Score = rawScore - scoreOffset
    [Header("Score")] 
    private int rawScore = 0; // the score (height) reported by the pins
    private int scoreOffset; // the starting initial score given by the highest pin on start.
    public int highScore { get; private set; }
    private string scorePath = "/score.json";

    [Header("Pause")] 
    public bool isPaused { get; private set; }

    //Scene Change events (to be called before scene change)
    public event Action OnRestartLevel;
    public event Action OnExitToMainMenu;
    
    public event Action<int> OnScoreChanged;

    //Initialize the singleton
    public void Awake()
    {
        base.Awake();
    }

    protected override void OnInitialize()
    {
        //OnRestartLevel initialization
        OnRestartLevel += SaveHighScoreToFile;
        OnRestartLevel += () => rawScore = 0;

        //OnExitToMainMenu initialization
        OnExitToMainMenu += () => SetPause(false);

        //Load the high score from file
        string path = Application.persistentDataPath + scorePath;
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            ScoreData data = JsonUtility.FromJson<ScoreData>(json);
            highScore = data.score;
        }
        
        OnScoreChanged += s => print(s);
    }

    protected override void OnReset()
    {
        RestartLevel();
    }

    protected override void OnCleanup()
    {
        // Clear event subscribers to prevent memory leaks
        OnRestartLevel = null;
        OnExitToMainMenu = null;
        
        // Reset pause state
        SetPause(false);
    }

    public void RestartLevel()
    {
        OnRestartLevel?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnApplicationQuit() => SaveHighScoreToFile();

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
        OnScoreChanged?.Invoke(GetScore());
    }

    public int GetScore()
    {
        return math.clamp(rawScore - scoreOffset, 0, int.MaxValue);
    }

    public void SetPause(bool pauseState)
    {
        if (isPaused == pauseState) return;
        isPaused = pauseState;
        Time.timeScale = isPaused ? 0 : 1;
        if (ManagersLoader.UI != null)
        {
            ManagersLoader.UI.PauseMenu(isPaused);
        }
    }

    public void ExitToMainMenu()
    {
        OnExitToMainMenu?.Invoke();
        SceneManager.LoadScene("Main Menu");
    }
}