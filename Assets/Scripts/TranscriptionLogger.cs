using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using UI;

public class TranscriptionLogger : MonoBehaviour
{
    #region UI

    [SerializeField] private TimerController _timerController;
    [SerializeField] private TextMeshProUGUI _fullText; // complete sentence with no pauses

    #endregion

    #region Transcriptions

    private string _previousText; // previously saved text
    private List<string> transcriptionList = new List<string>(); // full transcript (without timestamps)
    private List<(string, string)> transcriptionTimeList = new List<(string, string)>(); // full transcript (with timestamps)

    #endregion

    #region Speaking Pace

    private float sectionInterval;
    private float updateWpmInterval;
    private float lastWpmUpdateTime;
    private List<float> sectionAverages;
    private float sectionStartTime;
    private int sectionWordCount;
    private float overallAverageWPM;
    private int totalWordCount;
    private float startTime;
    private bool isFirstSectionProcessed;

    #endregion

    #region Unity

    private void Start()
    {
        // transcriptions
        _previousText = string.Empty;

        // speaking pace
        sectionInterval = 10.0f; // each section is 10 seconds
        updateWpmInterval = 2.0f; // update wpm every 2 seconds
        lastWpmUpdateTime = Time.time;
        sectionAverages = new List<float>();
        sectionStartTime = Time.time;
        sectionWordCount = 0;
        overallAverageWPM = 0.0f;
        totalWordCount = 0;
        startTime = Time.time;
        isFirstSectionProcessed = false;
    }

    private void Update()
    {
        // Component checks
        if (_fullText == null || _timerController == null)
        {
            return;
        }

        if (!_timerController.IsRunning())
        {
            return;
        }

        // Update the transcription list and word counts
        string currentText = _fullText.text;
        if (currentText != _previousText)
        {
            UpdateTranscriptionLists(currentText);
            UpdateWordCounts(currentText);
            _previousText = currentText;
        }

        // Calculate the section average WPM
        if (Time.time - sectionStartTime >= sectionInterval)
        {
            if (!isFirstSectionProcessed)
            {
                isFirstSectionProcessed = true;
                sectionStartTime = Time.time; // Reset start time for the first interval
                return; // Skip the first premature calculation
            }
            float wpm = (sectionWordCount / sectionInterval) * 60.0f;
            sectionAverages.Add(wpm);
            sectionStartTime = Time.time;
            sectionWordCount = 0;
        }

        // Calculate the overall average WPM
        if (Time.time - lastWpmUpdateTime >= updateWpmInterval)
        {
            float elapsedTime = Time.time - startTime;
            overallAverageWPM = (totalWordCount / elapsedTime) * 60.0f;
            lastWpmUpdateTime = Time.time;
        }
    }

    #endregion

    #region Helper Methods
    private void UpdateTranscriptionLists(string currentText)
    {
        transcriptionList.Add(currentText);
        string currentTime = _timerController.GetCurrentTime();
        transcriptionTimeList.Add((currentTime, currentText)); // logged time is when the sentence ended
    }

    private void UpdateWordCounts(string currentText)
    {
        if (string.IsNullOrWhiteSpace(currentText))
        {
            return;
        }
        string[] words = currentText.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        sectionWordCount += words.Length;
        totalWordCount += words.Length;
    }

    // Reset the transcription lists and word counts
    public void ResetTranscript()
    {
        // transcriptions
        _previousText = string.Empty;
        transcriptionList.Clear();
        transcriptionTimeList.Clear();

        // speaking pace
        sectionAverages.Clear();
        sectionStartTime = Time.time;
        sectionWordCount = 0;
        overallAverageWPM = 0;
        startTime = Time.time;
        totalWordCount = 0;

        // UI text
        if (_fullText != null)
        {
            _fullText.text = string.Empty;
        }
    }

    #endregion

    #region Getters
    public List<string> GetTranscriptionList()
    {
        return transcriptionList;
    }

    public List<(string, string)> GetTranscriptionTimeList()
    {
        return transcriptionTimeList;
    }

    public List<float> GetSectionAverages()
    {
        return sectionAverages;
    }

    public float GetOverallAverage()
    {
        return overallAverageWPM;
    }

    #endregion
}
