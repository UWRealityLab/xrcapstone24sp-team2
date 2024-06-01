using UI;
using UnityEngine;
using UnityEngine.Events;

public class TranscriptionProcessor : MonoBehaviour
{
    [SerializeField] private TranscriptionLogger transcriptionLogger;
    public UnityEvent onTranscriptionReady = new UnityEvent();
    public PodiumController podiumController;
    public void NotifyTranscriptionReady()
    {
        if (!podiumController.done())
        {
            Debug.Log("Transcription is ready for processing.");
            onTranscriptionReady.Invoke();
        }
    }

    // public void NotifyTranscriptionReady()
    // {
    //     Debug.Log("Transcription is ready for processing.");
    //     onTranscriptionReady.Invoke();
    // }
}