using UnityEngine;
using UnityEngine.Events;

public class TranscriptionProcessor : MonoBehaviour
{
    [SerializeField] private TranscriptionLogger transcriptionLogger;
    public UnityEvent onTranscriptionReady = new UnityEvent();

    public void NotifyTranscriptionReady()
    {
        Debug.Log("Transcription is ready for processing.");
        transcriptionLogger.AddEndOfTranscriptMarker();
        onTranscriptionReady.Invoke();
    }

    // public void NotifyTranscriptionReady()
    // {
    //     Debug.Log("Transcription is ready for processing.");
    //     onTranscriptionReady.Invoke();
    // }
}