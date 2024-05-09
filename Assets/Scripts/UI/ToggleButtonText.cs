using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class ToggleButtonText : MonoBehaviour
{
    public TextMeshProUGUI recordingText;
    public string startRecordingText = "Start Recording";
    public string stopRecordingText = "Stop Recording";
    public bool isRecording = false;

    public UnityEvent recordingFinished = new();

    // Toggle the recording state and text
    public void ToggleRecording()
    {
        isRecording = !isRecording;
        if (!isRecording) {
            recordingFinished.Invoke();
        }
        recordingText.text = isRecording ? stopRecordingText : startRecordingText;
    }

}
