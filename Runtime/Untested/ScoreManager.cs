using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple score manager to track and update a score value.
/// Can be attached to players to track individual scores or to a manager object for global score.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [Tooltip("Starting score value")]
    public int StartingScore = 0;
    
    [Tooltip("Current score (will be set to starting score on start)")]
    public int CurrentScore;
    
    [Tooltip("Highest score achieved (updated when current score exceeds it)")]
    public int HighScore = 0;
    
    [Tooltip("Target score to reach (for win conditions, 0 = no target)")]
    public int TargetScore = 0;
    
    [Header("Multiplier")]
    [Tooltip("Current score multiplier")]
    public float ScoreMultiplier = 1f;
    
    [Tooltip("Should score be rounded to nearest integer after applying multiplier")]
    public bool RoundScore = true;
    
    [Header("Events")]
    [Tooltip("Event triggered when score changes")]
    public UnityEvent<int> OnScoreChanged;
    
    [Tooltip("Event triggered when high score is beaten")]
    public UnityEvent<int> OnNewHighScore;
    
    [Tooltip("Event triggered when target score is reached")]
    public UnityEvent OnTargetReached;
    
    [Header("PlayerPrefs")]
    [Tooltip("Save high score to PlayerPrefs")]
    public bool SaveHighScore = true;
    
    [Tooltip("Key to use for saving high score in PlayerPrefs")]
    public string HighScoreKey = "HighScore";
    
    // Initialize the score
    private void Start()
    {
        // Set the current score to the starting score
        CurrentScore = StartingScore;
        
        // Load the high score from PlayerPrefs if saving is enabled
        if (SaveHighScore)
        {
            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }
    }
    
    /// <summary>
    /// Add to the current score
    /// </summary>
    /// <param name="amount">Amount to add (before multiplier)</param>
    public void AddScore(int amount)
    {
        // Apply score multiplier
        float multipliedScore = amount * ScoreMultiplier;
        
        // Round if specified
        int scoreToAdd = RoundScore ? Mathf.RoundToInt(multipliedScore) : Mathf.FloorToInt(multipliedScore);
        
        // Update the score
        CurrentScore += scoreToAdd;
        
        // Trigger the score changed event
        OnScoreChanged?.Invoke(CurrentScore);
        
        // Check if this is a new high score
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            
            // Save high score if enabled
            if (SaveHighScore)
            {
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }
            
            // Trigger the new high score event
            OnNewHighScore?.Invoke(HighScore);
        }
        
        // Check if target score reached
        if (TargetScore > 0 && CurrentScore >= TargetScore)
        {
            // Trigger the target reached event
            OnTargetReached?.Invoke();
        }
    }
    
    /// <summary>
    /// Subtract from the current score
    /// </summary>
    /// <param name="amount">Amount to subtract</param>
    public void SubtractScore(int amount)
    {
        // Update the score
        CurrentScore -= amount;
        
        // Trigger the score changed event
        OnScoreChanged?.Invoke(CurrentScore);
    }
    
    /// <summary>
    /// Set the current score to a specific value
    /// </summary>
    /// <param name="score">New score value</param>
    public void SetScore(int score)
    {
        // Set the score
        CurrentScore = score;
        
        // Trigger the score changed event
        OnScoreChanged?.Invoke(CurrentScore);
        
        // Check if this is a new high score
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            
            // Save high score if enabled
            if (SaveHighScore)
            {
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                PlayerPrefs.Save();
            }
            
            // Trigger the new high score event
            OnNewHighScore?.Invoke(HighScore);
        }
        
        // Check if target score reached
        if (TargetScore > 0 && CurrentScore >= TargetScore)
        {
            // Trigger the target reached event
            OnTargetReached?.Invoke();
        }
    }
    
    /// <summary>
    /// Reset the score to the starting value
    /// </summary>
    public void ResetScore()
    {
        SetScore(StartingScore);
    }
    
    /// <summary>
    /// Set the score multiplier
    /// </summary>
    /// <param name="multiplier">New multiplier value</param>
    public void SetMultiplier(float multiplier)
    {
        ScoreMultiplier = multiplier;
    }
    
    /// <summary>
    /// Reset the high score to zero
    /// </summary>
    public void ResetHighScore()
    {
        HighScore = 0;
        
        if (SaveHighScore)
        {
            PlayerPrefs.SetInt(HighScoreKey, 0);
            PlayerPrefs.Save();
        }
    }
}
