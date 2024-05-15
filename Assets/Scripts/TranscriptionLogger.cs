using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UI;

public class TranscriptionLogger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textComponent; // UI component to display the transcript
    [SerializeField] private TimerController _timerController; // Controller to manage timing

    private string _previousText; // Variable to store the text from the last update
    private List<string> transcriptionList; // List to store the full transcript (without timestamps)
    private List<(string, string)> transcriptionTimeList; // List to store the full transcript (with timestamps)
    
    // Speaking pace calculation
    private List<float> sectionAverages = new List<float>(); // Average speaking pace per section
    private float sectionStartTime; // Time when the current section started
    private float updateInterval = 10.0f; // Duration for each section in seconds (using a shorter interval for testing)
    private int wordCountThisSection = 0; // Word count in the current section
    private int totalWordCount = 0; // Total words counted
    private float totalTimeElapsed = 0; // Total time elapsed since the start of the section

    private void Start()
    {
        _previousText = string.Empty;
        transcriptionList = new List<string>();
        transcriptionTimeList = new List<(string, string)>();
        sectionStartTime = Time.time; // Initialize the start time of the first section
    }

    private void Update()
    {
        if (_textComponent == null)
        {
            Debug.LogError("TranscriptionLogger: _textComponent is not assigned!");
            return;
        }

        if (_timerController == null)
        {
            Debug.LogError("TranscriptionLogger: _timerController is not assigned!");
            return;
        }

        string currentText = _textComponent.text;
        if (currentText != _previousText)
        {
            UpdateTranscriptionLists(currentText);
            UpdateWordCounts(currentText);
            _previousText = currentText;
        }

        if (Time.time - sectionStartTime >= updateInterval)
        {
            CalculateSectionAverage();
        }
    }

    // Update the transcription list and the timestamp list
    private void UpdateTranscriptionLists(string currentText)
    {
        transcriptionList.Add(currentText);
        string currentTime = _timerController.GetCurrentTime();
        transcriptionTimeList.Add((currentTime, currentText));
    }

    // Update word counts for the current and overall counts
    private void UpdateWordCounts(string currentText)
    {
        int newWords = CountWords(currentText) - CountWords(_previousText);
        wordCountThisSection += newWords;
        totalWordCount += newWords;
        totalTimeElapsed = Time.time - sectionStartTime;
    }

    // Calculate the average words per minute for a section
    private void CalculateSectionAverage()
    {
        if (totalTimeElapsed > 0)
        {
            float wordsPerMinute = (wordCountThisSection / totalTimeElapsed) * 60;
            sectionAverages.Add(wordsPerMinute);
            wordCountThisSection = 0; // Reset word count for the next section
            sectionStartTime = Time.time; // Update start time for the next section
        }
    }

    private int CountWords(string text)
    {
        return text.Split(new char[] { ' ', '\n' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
    }

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
        if (totalTimeElapsed > 0)
        {
            return (totalWordCount / totalTimeElapsed) * 60;
        }
        return 0;
    }
}
