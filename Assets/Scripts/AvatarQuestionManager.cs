using System;
using System.Collections.Generic;
using UnityEngine;

public class AvatarQuestionManager : MonoBehaviour
{
    [SerializeField] private OpenAITTS tts;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string personaType;
    public string CurrentVoice { get; private set; }
    // private List<string> allQuestions = new List<string>();
    public List<string> allQuestions { get; private set; } = new List<string>();

    private bool isQuestionBeingProcessed = false;

    void Start()
    {
        AvatarCreator.OnAvatarDataCreated += HandleAvatarDataReceived;
    }

    void OnDestroy()
    {
        AvatarCreator.OnAvatarDataCreated -= HandleAvatarDataReceived;
    }

    private void HandleAvatarDataReceived(AvatarCreator.AvatarData avatarData)
    {
        if (avatarData.Persona == personaType)
        {
            Debug.Log($"Received data for: {avatarData.Persona}, Expected: {personaType}");
            UpdateData(avatarData);
        }
    }

    private void UpdateData(AvatarCreator.AvatarData avatarData)
    {
        CurrentVoice = avatarData.Voice;
        allQuestions.Clear();
        foreach (var section in avatarData.Sections)
        {
            allQuestions.AddRange(section.Value);
        }
        Debug.Log("Data updated for " + avatarData.Persona + " with voice " + CurrentVoice);
    }

    public void AskRandomQuestion()
    {
        if (audioSource.isPlaying || isQuestionBeingProcessed || allQuestions.Count == 0)
        {
            Debug.Log("Waiting: Audio is still playing or question is being processed, or no questions are available.");
            return;
        }

        isQuestionBeingProcessed = true;
        int randomIndex = UnityEngine.Random.Range(0, allQuestions.Count);
        string questionText = allQuestions[randomIndex];

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
