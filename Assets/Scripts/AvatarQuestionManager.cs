using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AvatarQuestionManager : MonoBehaviour
{
    [SerializeField] private CommunicationManager communicationManager; // Reference to the CommunicationManager
    [SerializeField] private OpenAITTS tts;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private TextMeshProUGUI questions;
    [SerializeField] private string personaType;
    [SerializeField] private GameObject questionButton;
    public string CurrentVoice { get; private set; }
    public List<string> allQuestions { get; private set; } = new List<string>();
    private Queue<string> questionQueue = new Queue<string>();

    private bool isQuestionBeingProcessed = false;

    void Start()
    {
        communicationManager.OnAvatarDataReady.AddListener(HandleAvatarDataReceived);
    }

    void OnDestroy()
    {
        communicationManager.OnAvatarDataReady.RemoveListener(HandleAvatarDataReceived);
    }

    private void HandleAvatarDataReceived(CommunicationManager.AvatarData avatarData)
    {
        if (avatarData.Persona == personaType)
        {
            Debug.Log($"Received data for: {avatarData.Persona}, Expected: {personaType}");
            UpdateData(avatarData);
        }
    }
    private void UpdateData(CommunicationManager.AvatarData avatarData)
    {
        CurrentVoice = avatarData.Voice;
        questionQueue.Clear();
        foreach (var section in avatarData.Sections)
        {
            foreach (var question in section.Value)
            {
                questionQueue.Enqueue(question);
            }
        }
        Debug.Log($"Data updated for {avatarData.Persona} with voice {CurrentVoice}. Total questions loaded: {questionQueue.Count}");
    }
    // private void UpdateData(CommunicationManager.AvatarData avatarData)
    // {
    //     CurrentVoice = avatarData.Voice;
    //     allQuestions.Clear();
    //     foreach (var section in avatarData.Sections)
    //     {
    //         allQuestions.AddRange(section.Value);
    //     }
    //     foreach (string q in allQuestions) {
    //         questions.text = q + "\n";
    //     }
    //     Debug.Log("Data updated for " + avatarData.Persona + " with voice " + CurrentVoice);
    // }

    public void AskQuestion()
    {
        if (audioSource.isPlaying || isQuestionBeingProcessed || questionQueue.Count == 0)
        {
            Debug.Log("Waiting: Audio is still playing, question is being processed, or no questions are available.");
            return;
        }

        isQuestionBeingProcessed = true;
        string questionText = questionQueue.Dequeue(); // Get the next question

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
    // public void AskRandomQuestion()
    // {
    //     if (audioSource.isPlaying || isQuestionBeingProcessed || allQuestions.Count == 0)
    //     {
    //         Debug.Log("Waiting: Audio is still playing or question is being processed, or no questions are available.");
    //         return;
    //     }

    //     isQuestionBeingProcessed = true;
    //     int randomIndex = UnityEngine.Random.Range(0, allQuestions.Count);
    //     string questionText = allQuestions[randomIndex];

    //     string jsonData = JsonUtility.ToJson(new TTSRequestData
    //     {
    //         model = "tts-1",
    //         input = questionText,
    //         voice = CurrentVoice
    //     });

    //     StartCoroutine(tts.GetTTS(jsonData, clip =>
    //     {
    //         isQuestionBeingProcessed = false;
    //         if (clip != null)
    //         {
    //             audioSource.clip = clip;
    //             audioSource.Play();
    //             Debug.Log("Playing audio for question: " + questionText);
    //         }
    //         else
    //         {
    //             Debug.LogError("Failed to load audio for question: " + questionText);
    //         }
    //     }));
    // }

    [Serializable]
    public class TTSRequestData
    {
        public string model;
        public string input;
        public string voice;
    }

    public void ShowQuestionButton()
    {
        questionButton.SetActive(true);
    }

    public void HideQuestionButton()
    {
        questionButton.SetActive(false);
    }
}