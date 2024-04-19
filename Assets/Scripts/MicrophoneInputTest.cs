using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Record audio from the microphone and play it back in real-time (for testing)
[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    AudioSource audioSource;
    private bool isMicStarted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        StartRecording();
    }

    public void StartRecording()
    {
        // Ensure we don't start the microphone more than once
        if (!isMicStarted)
        {
            audioSource.clip = Microphone.Start(null, true, 10, 44100);
            // Wait until the microphone has recorded something
            while (!(Microphone.GetPosition(null) > 0)) {}
            audioSource.Play();
            isMicStarted = true;
            Debug.Log("Microphone is recording.");
        }
    }

    public void StopRecording()
    {
        if (isMicStarted)
        {
            Microphone.End(null);
            audioSource.Stop();
            isMicStarted = false;
            Debug.Log("Microphone stopped recording.");
        }
    }

    // Toggle between starting and stopping recording
    public void ToggleRecording()
    {
        if (isMicStarted)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    void OnDisable()
    {
        StopRecording();
    }
}
