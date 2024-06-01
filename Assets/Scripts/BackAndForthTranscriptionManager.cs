using System;
using UnityEngine;
using TMPro;

public class BackAndForthTranscriptionManager : MonoBehaviour
{
    [SerializeField] private TranscriptionLogger transcriptionLogger;
    [SerializeField] private TextMeshProUGUI timerText; // Reference to the timer UI TextMeshPro
    [SerializeField] private TextMeshProUGUI fullTranscriptText; // Reference to the full transcript UI TextMeshPro
    private DateTime startTime;
    private DateTime endTime;

    public void StartTimer()
    {
        startTime = DateTime.Now;
    }

    public void StopTimer()
    {
        endTime = DateTime.Now;
    }

    public string GetRelevantTranscript()
    {
        // Get the full transcript
        string fullTranscript = string.Join("\n", transcriptionLogger.GetTranscriptionList());
        return fullTranscript;
    }

    // public string GetRelevantTranscript()
    // {
    //     // Get the full transcript
    //     string fullTranscript = string.Join("\n", transcriptionList);
    //     return fullTranscript;
    // }
    // public string GetRelevantTranscript()
    // {
    //     // Get the full transcript
    //     string fullTranscript = fullTranscriptText.text;
        
    //     // Extract relevant part of the transcript based on timestamps
    //     string relevantTranscript = ExtractRelevantTranscript(fullTranscript, startTime, endTime);
        
    //     return relevantTranscript;
    // }

    private string ExtractRelevantTranscript(string fullTranscript, DateTime start, DateTime end)
    {
        // Assuming fullTranscript contains timestamps in the format [HH:mm:ss] at the start of each line
        string[] lines = fullTranscript.Split('\n');
        string relevantTranscript = "";

        foreach (var line in lines)
        {
            if (line.Length < 9) continue; // Skip lines that are too short to contain a timestamp

            // Extract the timestamp
            string timestampStr = line.Substring(1, 8); // Assuming timestamp is in the format [HH:mm:ss]
            if (DateTime.TryParseExact(timestampStr, "HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime timestamp))
            {
                if (timestamp >= start && timestamp <= end)
                {
                    relevantTranscript += line + "\n";
                }
            }
        }

        return relevantTranscript;
    }
}