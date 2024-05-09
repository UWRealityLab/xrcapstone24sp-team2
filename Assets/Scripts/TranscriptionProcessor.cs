using UnityEngine;
using UnityEngine.Events;

public class TranscriptionProcessor : MonoBehaviour
{
    public UnityEvent onTranscriptionReady = new UnityEvent();

    public void NotifyTranscriptionReady()
    {
        Debug.Log("Transcription is ready for processing.");
        onTranscriptionReady.Invoke();
    }
}