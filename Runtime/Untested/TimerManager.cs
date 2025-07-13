using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple timer system that can count up or down.
/// Useful for creating time limits or tracking elapsed time.
/// </summary>
public class TimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Should the timer count down (true) or up (false)")]
    public bool CountDown = true;
    
    [Tooltip("Timer duration in seconds")]
    public float Duration = 60f;
    
    [Tooltip("Current time value (will be set to Duration on start if counting down)")]
    public float CurrentTime = 0f;
    
    [Tooltip("Should the timer start automatically on game start")]
    public bool StartAutomatically = true;
    
    [Tooltip("Should the timer loop when it finishes")]
    public bool Loop = false;
    
    [Header("Timer Formatting")]
    [Tooltip("Format to display the time (Simple = SS, Standard = MM:SS, Detailed = HH:MM:SS)")]
    public TimeFormat DisplayFormat = TimeFormat.Standard;
    
    [Header("Events")]
    [Tooltip("Event triggered when the timer starts")]
    public UnityEvent OnTimerStart;
    
    [Tooltip("Event triggered when the timer is paused")]
    public UnityEvent OnTimerPause;
    
    [Tooltip("Event triggered when the timer is reset")]
    public UnityEvent OnTimerReset;
    
    [Tooltip("Event triggered when the timer finishes")]
    public UnityEvent OnTimerFinish;
    
    [Tooltip("Event triggered every second with the current time")]
    public UnityEvent<float> OnSecondPassed;
    
    [Tooltip("Event triggered every frame with the current time")]
    public UnityEvent<float> OnTimeChanged;
    
    [Tooltip("Event triggered with formatted time string when time changes")]
    public UnityEvent<string> OnFormattedTimeChanged;
    
    // Whether the timer is currently running
    private bool _isRunning = false;
    
    // Track the last second for the OnSecondPassed event
    private int _lastSecond = -1;
    
    // Format options for displaying time
    public enum TimeFormat
    {
        Simple,     // Just seconds (SS)
        Standard,   // Minutes and seconds (MM:SS)
        Detailed    // Hours, minutes, seconds (HH:MM:SS)
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the timer
        if (CountDown)
        {
            CurrentTime = Duration;
        }
        else
        {
            CurrentTime = 0f;
        }
        
        // Start the timer if set to start automatically
        if (StartAutomatically)
        {
            StartTimer();
        }
        else
        {
            // Still trigger the initial time changed events
            TriggerTimeChangedEvents();
        }
    }
    
    // Update is called once per frame
    private void Update()
    {
        if (_isRunning)
        {
            // Update the timer based on direction
            if (CountDown)
            {
                CurrentTime -= Time.deltaTime;
                
                // Check if timer finished
                if (CurrentTime <= 0f)
                {
                    FinishTimer();
                }
            }
            else
            {
                CurrentTime += Time.deltaTime;
                
                // Check if timer reached duration
                if (CurrentTime >= Duration && Duration > 0f)
                {
                    FinishTimer();
                }
            }
            
            // Trigger time changed events
            TriggerTimeChangedEvents();
        }
    }
    
    // Trigger the appropriate timer events
    private void TriggerTimeChangedEvents()
    {
        // Trigger the time changed event
        OnTimeChanged?.Invoke(CurrentTime);
        
        // Trigger the formatted time changed event
        OnFormattedTimeChanged?.Invoke(FormatTime(CurrentTime));
        
        // Check if a new second has passed
        int currentSecond = Mathf.FloorToInt(CurrentTime);
        if (currentSecond != _lastSecond)
        {
            _lastSecond = currentSecond;
            OnSecondPassed?.Invoke(CurrentTime);
        }
    }
    
    // Handle timer completion
    private void FinishTimer()
    {
        // Clamp the time
        if (CountDown)
        {
            CurrentTime = 0f;
        }
        else
        {
            CurrentTime = Duration;
        }
        
        // Pause the timer
        _isRunning = false;
        
        // Trigger the finish event
        OnTimerFinish?.Invoke();
        
        // If looping is enabled, reset and restart the timer
        if (Loop)
        {
            ResetTimer();
            StartTimer();
        }
    }
    
    /// <summary>
    /// Start or resume the timer
    /// </summary>
    public void StartTimer()
    {
        if (!_isRunning)
        {
            _isRunning = true;
            OnTimerStart?.Invoke();
        }
    }
    
    /// <summary>
    /// Pause the timer
    /// </summary>
    public void PauseTimer()
    {
        if (_isRunning)
        {
            _isRunning = false;
            OnTimerPause?.Invoke();
        }
    }
    
    /// <summary>
    /// Toggle between pause and resume
    /// </summary>
    public void ToggleTimer()
    {
        if (_isRunning)
        {
            PauseTimer();
        }
        else
        {
            StartTimer();
        }
    }
    
    /// <summary>
    /// Reset the timer to its initial state
    /// </summary>
    public void ResetTimer()
    {
        // Reset the time based on direction
        if (CountDown)
        {
            CurrentTime = Duration;
        }
        else
        {
            CurrentTime = 0f;
        }
        
        // Trigger the reset event
        OnTimerReset?.Invoke();
        
        // Update displayed time
        TriggerTimeChangedEvents();
    }
    
    /// <summary>
    /// Add time to the timer
    /// </summary>
    /// <param name="seconds">Seconds to add</param>
    public void AddTime(float seconds)
    {
        if (CountDown)
        {
            CurrentTime += seconds;
            
            // Cap at duration if needed
            if (CurrentTime > Duration)
            {
                CurrentTime = Duration;
            }
        }
        else
        {
            CurrentTime -= seconds;
            
            // Cap at zero if needed
            if (CurrentTime < 0f)
            {
                CurrentTime = 0f;
            }
        }
        
        // Update displayed time
        TriggerTimeChangedEvents();
    }
    
    /// <summary>
    /// Format the time according to the selected format
    /// </summary>
    /// <param name="timeToFormat">Time in seconds to format</param>
    /// <returns>Formatted time string</returns>
    public string FormatTime(float timeToFormat)
    {
        // Ensure time is not negative
        timeToFormat = Mathf.Max(0f, timeToFormat);
        
        int hours = Mathf.FloorToInt(timeToFormat / 3600f);
        int minutes = Mathf.FloorToInt((timeToFormat % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeToFormat % 60f);
        
        switch (DisplayFormat)
        {
            case TimeFormat.Simple:
                return seconds.ToString();
                
            case TimeFormat.Standard:
                return string.Format("{0:00}:{1:00}", minutes, seconds);
                
            case TimeFormat.Detailed:
                return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
                
            default:
                return timeToFormat.ToString("F2");
        }
    }
    
    /// <summary>
    /// Check if the timer is currently running
    /// </summary>
    /// <returns>True if running, false if paused</returns>
    public bool IsRunning()
    {
        return _isRunning;
    }
    
    /// <summary>
    /// Check if the timer has finished
    /// </summary>
    /// <returns>True if finished, false otherwise</returns>
    public bool IsFinished()
    {
        if (CountDown)
        {
            return CurrentTime <= 0f;
        }
        else
        {
            return CurrentTime >= Duration && Duration > 0f;
        }
    }
}
