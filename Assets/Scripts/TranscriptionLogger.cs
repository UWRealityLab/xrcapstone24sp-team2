using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using UI;

public class TranscriptionLogger : MonoBehaviour
{
    #region UI

    [SerializeField]
    private TimerController _timerController;
    [SerializeField]
    private TextMeshProUGUI _partialText; // partial transcription

    #endregion

    #region Transcriptions

    private string _previousText; // previously saved text
    private List<string> transcriptionList; // full transcript (without timestamps)
    private List<(string, string)> transcriptionTimeList; // full transcript (with timestamps)

    private string _previousResponseText; // previously saved response text
    private List<string> responseList; // full response transcript

    private float updateTranscriptionInterval;
    private float lastTranscriptionUpdateTime;

    private bool isResponseActive;

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
        updateTranscriptionInterval = 0.5f;
        lastTranscriptionUpdateTime = _timerController.GetElapsedTimeInSeconds();

        // response
        _previousResponseText = string.Empty;
        responseList = new List<string>();
        isResponseActive = false;

        // section
        sectionInterval = 10.0f; // each section is 10 seconds
        sectionWordCount = 0;
        sectionStartTime = _timerController.GetElapsedTimeInSeconds();
        sectionAverages = new List<float>();

        // total
        updateWpmInterval = 2.0f; // update wpm every 2 seconds
        totalWordCount = 0;
        lastWpmUpdateTime = _timerController.GetElapsedTimeInSeconds();
        startTime = _timerController.GetElapsedTimeInSeconds();
        overallAverageWPM = 0.0f;
    }

    private void Update()
    {
        // Component checks
        if (_timerController == null || _partialText == null)
        {
            return;
        }

        // Handle response transcription
        if (isResponseActive)
        {
            if (_partialText != null)
            {
                string currentResponseText = _partialText.text;
                if (currentResponseText != _previousResponseText)
                {
                    UpdateResponseTranscriptionLists(currentResponseText);
                    _previousResponseText = currentResponseText;
                }
            }
            return;
        }

        // Check if the timer is running
        if (!_timerController.IsRunning())
        {
            return;
        }

        // Update transcription lists (with and without timestamps) and word counts (section and total)
        string currentText = _partialText.text;
        if (_timerController.GetElapsedTimeInSeconds() - lastTranscriptionUpdateTime >= updateTranscriptionInterval)
        {
            if (currentText != _previousText)
            {
                UpdateTranscriptionLists(currentText);
                UpdateWordCounts(currentText);
                _previousText = currentText;
            }
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
        string newText = string.Empty;

        if (!string.IsNullOrEmpty(_previousText) && currentText.StartsWith(_previousText))
        {
            newText = currentText.Substring(_previousText.Length).Trim();
        }
        else
        {
            newText = currentText;
        }

        if (!string.IsNullOrEmpty(newText))
        {
            transcriptionList.Add(newText);
            string currentTime = _timerController.GetCurrentTime();
            transcriptionTimeList.Add((currentTime, newText));
        }
    }

    private void UpdateWordCounts(string currentText)
    {
        if (string.IsNullOrWhiteSpace(currentText))
        {
            return;
        }

        string newText = string.Empty;

        if (!string.IsNullOrEmpty(_previousText) && currentText.StartsWith(_previousText))
        {
            newText = currentText.Substring(_previousText.Length).Trim();
        }
        else
        {
            newText = currentText;
        }

        if (!string.IsNullOrEmpty(newText))
        {
            string[] words = newText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            sectionWordCount += words.Length;
            totalWordCount += words.Length;
        }
    }

    private void UpdateResponseTranscriptionLists(string currentResponseText)
    {
        string newText = string.Empty;

        if (!string.IsNullOrEmpty(_previousResponseText) && currentResponseText.StartsWith(_previousResponseText))
        {
            newText = currentResponseText.Substring(_previousResponseText.Length).Trim();
        }
        else
        {
            newText = currentResponseText;
        }

        if (!string.IsNullOrEmpty(newText))
        {
            responseList.Add(newText);
            string currentTime = _timerController.GetCurrentTime();
        }
    }

    public void SetResponseActive(bool isActive)
    {
        isResponseActive = isActive;
    }

    public void AddEndOfTranscriptMarker()
    {
        string endMarker = "###Transcript end###";
        transcriptionList.Add(endMarker);
        string currentTime = _timerController.GetCurrentTime();
        transcriptionTimeList.Add((currentTime, endMarker));
    }

    // Reset the transcription lists and word counts
    public void ResetTranscript()
    {
        // transcriptions
        _previousText = string.Empty;
        transcriptionList.Clear();
        transcriptionTimeList.Clear();

        // response
        ResetResponseTranscript();

        // speaking pace
        sectionAverages.Clear();
        sectionStartTime = _timerController.GetElapsedTimeInSeconds();
        sectionWordCount = 0;
        overallAverageWPM = 0;
        startTime = _timerController.GetElapsedTimeInSeconds();
        totalWordCount = 0;

        // UI text
        if (_partialText != null)
        {
            _partialText.text = string.Empty;
        }
    }

    public void ResetResponseTranscript()
    {
        _previousResponseText = string.Empty;
        responseList.Clear();
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

    public List<string> GetResponseList()
    {
        return responseList;
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
