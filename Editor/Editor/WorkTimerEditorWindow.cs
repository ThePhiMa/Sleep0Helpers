using System;
using UnityEditor;
using UnityEngine;

public class WorkTimer : EditorWindow
{
    private bool isRunning = false;
    private float elapsedTime = 0f;
    private bool autoStartEnabled = false;
    private bool autoStopEnabled = false;
    private DateTime lastUpdateTime;

    private const string ElapsedTimeKey = "WorkTimer_ElapsedTime";
    private const string AutoStartKey = "WorkTimer_AutoStart";
    private const string AutoStopKey = "WorkTimer_AutoStop";

    private bool wasEditorFocused = true;

    [MenuItem("Tools/Sleep0/Work Timer")]
    public static void ShowWindow()
    {
        GetWindow<WorkTimer>("Work Timer");
    }

    void OnEnable()
    {
        LoadState();
        EditorApplication.update += OnEditorUpdate;
    }

    void OnDisable()
    {
        SaveState();
        EditorApplication.update -= OnEditorUpdate;
    }

    void OnEditorUpdate()
    {
        bool isEditorFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;

        if (isEditorFocused != wasEditorFocused)
        {
            if (isEditorFocused)
            {
                OnEditorFocused();
            }
            else
            {
                OnEditorLostFocus();
            }
            wasEditorFocused = isEditorFocused;
        }

        if (isRunning)
        {
            UpdateTimer();
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Work Timer", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("Time:", FormatTime(elapsedTime));

        if (GUILayout.Button(isRunning ? "Stop" : "Start"))
        {
            ToggleTimer();
        }

        bool newAutoStartEnabled = EditorGUILayout.Toggle("Auto-start on focus", autoStartEnabled);
        if (newAutoStartEnabled != autoStartEnabled)
        {
            autoStartEnabled = newAutoStartEnabled;
            SaveState();
        }

        bool newAutoStopEnabled = EditorGUILayout.Toggle("Auto-stop on focus", autoStopEnabled);
        if (newAutoStopEnabled != autoStopEnabled)
        {
            autoStopEnabled = newAutoStopEnabled;
            SaveState();
        }

        if (GUILayout.Button("Reset"))
        {
            ResetTimer();
        }
    }

    private void OnEditorFocused()
    {
        if (autoStartEnabled && !isRunning)
        {
            Debug.Log("Start Timer");
            StartTimer();
        }
    }

    private void OnEditorLostFocus()
    {
        if (autoStopEnabled && isRunning)
        {
            Debug.Log("Stop Timer");
            StopTimer();
        }
        SaveState();
    }

    private void ToggleTimer()
    {
        if (isRunning)
        {
            StopTimer();
        }
        else
        {
            StartTimer();
        }
        SaveState();
    }

    private void StartTimer()
    {
        isRunning = true;
        lastUpdateTime = DateTime.Now;
    }

    private void StopTimer()
    {
        isRunning = false;
    }

    private void UpdateTimer()
    {
        DateTime currentTime = DateTime.Now;
        elapsedTime += (float)(currentTime - lastUpdateTime).TotalSeconds;
        lastUpdateTime = currentTime;
        Repaint();
    }

    private void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = false;
        SaveState();
    }

    private string FormatTime(float timeInSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    private void SaveState()
    {
        EditorPrefs.SetFloat(ElapsedTimeKey, elapsedTime);
        EditorPrefs.SetBool(AutoStartKey, autoStartEnabled);
        EditorPrefs.SetBool(AutoStopKey, autoStopEnabled);
    }

    private void LoadState()
    {
        elapsedTime = EditorPrefs.GetFloat(ElapsedTimeKey, 0f);
        autoStartEnabled = EditorPrefs.GetBool(AutoStartKey, false);
        autoStopEnabled = EditorPrefs.GetBool(AutoStopKey, false);
    }
}