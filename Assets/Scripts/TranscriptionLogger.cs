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
    private List<string> transcriptionList; // full transcript (without timestamps)
    private List<(string, string)> transcriptionTimeList; // full transcript (with timestamps)

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

    #endregion

    #region Unity

    private void Start()
    {
        // transcriptions
        _previousText = string.Empty;
        transcriptionList = new List<string>();
        transcriptionTimeList = new List<(string, string)>();

        // section
        sectionInterval = 10.0f; // each section is 10 seconds
        sectionWordCount = 0;
        sectionStartTime = _timerController.GetElapsedTimeInSeconds();
        sectionAverages = new List<float>();

        // total
        updateWpmInterval = 2.0f; // update wpm every 2 seconds
        lastWpmUpdateTime = _timerController.GetElapsedTimeInSeconds();
        totalWordCount = 0;
        startTime = _timerController.GetElapsedTimeInSeconds();
        overallAverageWPM = 0.0f;
    }

    private void Update()
    {
        // Component checks
        if (_timerController == null || _fullText == null)
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

        // Calculate the overall average WPM
        if (_timerController.GetElapsedTimeInSeconds() - lastWpmUpdateTime >= updateWpmInterval)
        {
            overallAverageWPM = (totalWordCount / (_timerController.GetElapsedTimeInSeconds() - startTime)) * 60.0f;
            lastWpmUpdateTime =  _timerController.GetElapsedTimeInSeconds();
        }

        // Calculate the section average WPM
        if (_timerController.GetElapsedTimeInSeconds() - sectionStartTime >= sectionInterval)
        {
            float wpm = (sectionWordCount / sectionInterval) * 60.0f;
            sectionAverages.Add(wpm);
            sectionStartTime = _timerController.GetElapsedTimeInSeconds();
            sectionWordCount = 0;
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
        sectionStartTime = _timerController.GetElapsedTimeInSeconds();
        sectionWordCount = 0;
        overallAverageWPM = 0;
        startTime = _timerController.GetElapsedTimeInSeconds();
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
