using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToggleButtonText : MonoBehaviour
{
    public TextMeshProUGUI recordingText;
    public string startRecordingText = "Start Recording";
    public string stopRecordingText = "Stop Recording";
    private bool isRecording = false;

    // Toggle the recording state and text
    public void ToggleRecording()
    {
        isRecording = !isRecording;
        recordingText.text = isRecording ? stopRecordingText : startRecordingText;
    }
}
