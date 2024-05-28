using UI;
using UnityEngine;
using UnityEngine.Events;

public class TranscriptionProcessor : MonoBehaviour
{
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
}