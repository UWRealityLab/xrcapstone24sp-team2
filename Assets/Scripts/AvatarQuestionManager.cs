using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class AvatarQuestionManager : MonoBehaviour
{
    [SerializeField] private OpenAITTS tts;
    [SerializeField] private AudioSource audioSource;
    public string CurrentVoice { get; private set; }

    public Dictionary<string, List<string>> SectionQuestions { get; private set; } = new Dictionary<string, List<string>>();
    private bool isQuestionBeingProcessed = false;

    public event Action OnDataReady;

    void Start()
    {
        // Subscribe to the updated event
        AvatarCreator.OnAvatarDataCreated += HandleAvatarDataReceived;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        AvatarCreator.OnAvatarDataCreated -= HandleAvatarDataReceived;
    }

    private void HandleAvatarDataReceived(AvatarCreator.AvatarData avatarData)
    {
        UpdateData(avatarData);  // This method will now setup data and trigger the OnDataReady event
    }

    private void UpdateData(AvatarCreator.AvatarData avatarData)
    {
        CurrentVoice = avatarData.Voice;
        SectionQuestions = avatarData.Sections;
        OnDataReady?.Invoke();  // Notify subscribers that new data is ready
        Debug.Log("Data updated for " + avatarData.Persona + " with voice " + CurrentVoice);
    }

    public void AskQuestion(string questionText)
    {
        if (audioSource.isPlaying || isQuestionBeingProcessed)
        {
            Debug.Log("Waiting: Audio is still playing or question is being processed.");
            return;
        }

        isQuestionBeingProcessed = true;

        // Use the voice that was loaded with the JSON object
        string jsonData = JsonUtility.ToJson(new TTSRequestData
        {
            model = "tts-1",
            input = questionText,
            voice = CurrentVoice
        });

        StartCoroutine(tts.GetTTS(jsonData, clip =>
        {
            isQuestionBeingProcessed = false;
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("Playing audio for question: " + questionText);
            }
            else
            {
                Debug.LogError("Failed to load audio for question: " + questionText);
            }
        }));
    }

    [System.Serializable]
    public class TTSRequestData
    {
        public string model;
        public string input;
        public string voice;
    }
}
