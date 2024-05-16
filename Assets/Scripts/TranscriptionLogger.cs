using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UI;

public class TranscriptionLogger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textComponent; // UI component to display the transcript
    [SerializeField] private TimerController _timerController; // Controller to manage timing

    [SerializeField] TextMeshProUGUI _partialTextComponent;
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
    private float overallAverageWPM = 0;
    private float updateWpmInterval = 2.0f; // Update every 2 seconds
    private float lastUpdateTime; // Time of the last update
    private float lastWpmUpdateTime; // Time of the last WPM update

    private void Start()
    {
        _previousText = string.Empty;
        transcriptionList = new List<string>();
        transcriptionTimeList = new List<(string, string)>();
        sectionStartTime = Time.time;
        lastUpdateTime = Time.time;
        lastWpmUpdateTime = 0;
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

        // Only proceed with updates if the timer is currently running
        if (!_timerController.IsRunning())
        {
            return;
        }

        // Update the transcription list and word counts if it has changed
        string currentText = _textComponent.text;
        if (currentText != _previousText)
        {
            UpdateTranscriptionLists(currentText);
            UpdateWordCounts(currentText);
            _previousText = currentText;
        }

        // Update the time elapsed and calculate the section average at regular intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            totalTimeElapsed += Time.time - lastUpdateTime;
            lastUpdateTime = Time.time;

            if (Time.time - sectionStartTime >= updateInterval)
            {
                CalculateSectionAverage();
            }
        }

        // Update the overall average WPM at regular intervals
        if (_timerController.GetElapsedTimeInSeconds() - lastWpmUpdateTime >= updateWpmInterval)
        {
            // Time to update
            overallAverageWPM = CalculateOverallAverage();
            lastWpmUpdateTime = _timerController.GetElapsedTimeInSeconds(); // Update lastWpmUpdateTime to the current time
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
        int newWords = 0;

        if (currentText.Length > _previousText.Length)
        {
            string addedText = currentText.Substring(_previousText.Length);
            newWords = CountWords(addedText);
        }

        wordCountThisSection += newWords;
        totalWordCount += newWords;
    }

    // Calculate the average words per minute for a section
    private void CalculateSectionAverage()
    {
        float timeElapsed = Time.time - sectionStartTime;

        // Ensure that the time elapsed is greater than a certain threshold (e.g., 1 second)
        if (timeElapsed > 1.0f)
        {
            float wordsPerMinute = (wordCountThisSection / timeElapsed) * 60.0f;
            sectionAverages.Add(wordsPerMinute);
        }

        // Reset the word count and start time for the next section
        wordCountThisSection = 0;
        sectionStartTime = Time.time;
    }

    // Count the number of words in a given text by splitting it based on spaces
    private int CountWords(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        return text.Split(' ').Length;
    }

    // Reset the transcription lists and word counts
    public void ResetTranscript()
    {
        _previousText = string.Empty;
        _textComponent.text = "Full transcription:";
        _partialTextComponent.text = "Partial transcription:";
        transcriptionList.Clear();
        transcriptionTimeList.Clear();
        sectionAverages.Clear();
        totalWordCount = 0;
        totalTimeElapsed = 0;
        wordCountThisSection = 0;
        sectionStartTime = 0;
        overallAverageWPM = 0;
        lastUpdateTime = 0;
        lastWpmUpdateTime = 0;
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

    // Calculate the overall average words per minute
    private float CalculateOverallAverage()
    {
        int totalWords = 0;
        foreach (string transcript in transcriptionList)
        {
            totalWords += CountWords(transcript);
        }

        float totalTimeInMinutes = _timerController.GetElapsedTimeInSeconds() / 60.0f;
        return totalWords / totalTimeInMinutes;
    }

    public float GetOverallAverage()
    {
        return overallAverageWPM;
    }
}
