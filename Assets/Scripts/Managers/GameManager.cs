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
    public int currentScore => GetScore();
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
        
//        OnScoreChanged += s => print(s);
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

    public event Action OnHitBySpike;
    public void HitBySpike()
    {
        OnHitBySpike?.Invoke();
        RestartLevel();
    }
    
    public event Action OnPinFellOffScreen;
    public void PinFellOffScreen()
    {
        OnPinFellOffScreen?.Invoke();
        RestartLevel();
    }

    public void RestartLevel()
    {
        OnRestartLevel?.Invoke();
        rawScore = scoreOffset;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        print($"Raw Score: {rawScore}, Score: {GetScore()}, highscore: {highScore}, scoreOffset: {scoreOffset}");
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
        OnScoreChanged?.Invoke(currentScore);
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
        rawScore = scoreOffset;
        SceneManager.LoadScene("Main Menu");
    }
    

    #region Debug Support Methods


    public void EnableGodMode()
    {
        
    }
    
    public void ModifyDifficulty(int change)
    {
        int currentDifficulty = PlayerPrefs.GetInt("difficulty", 0);
        int newDifficulty = Mathf.Clamp(currentDifficulty + change, 0, 1); // 0 = Easy, 1 = Hard
        PlayerPrefs.SetInt("difficulty", newDifficulty);
        PlayerPrefs.Save();
        Debug.Log($"Difficulty changed to: {(newDifficulty == 0 ? "Easy" : "Hard")}");
    }

    public void AddScore(int amount)
    {
        rawScore += amount;
        OnScoreChanged?.Invoke(GetScore());
        Debug.Log($"Score added: {amount}, new score: {GetScore()}");
    }

    public void ResetScore()
    {
        rawScore = scoreOffset;
        OnScoreChanged?.Invoke(GetScore());
        Debug.Log("Score reset to 0");
    }

    public void ModifyLives(int change)
    {
        
        Debug.Log($"Lives modified by: {change}");
    }

    public void ModifyGameTime(float seconds)
    {
        
        Debug.Log($"Game time modified by: {seconds}");
    }

    public void ResetGameSession()
    {
        ResetScore();
        Debug.Log("Game session reset");
    }

    #endregion
}