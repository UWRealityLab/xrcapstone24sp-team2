using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AvatarQuestionManager : MonoBehaviour
{
    [SerializeField] private CommunicationManager communicationManager; // Reference to the CommunicationManager
    [SerializeField] private OpenAITTS tts;
    [SerializeField] private AudioSource audioSource;
    // [SerializeField] private OutputAudioRecorder audioRecorder; // Reference to the OutputAudioRecorder
    [SerializeField] private BackAndForthTranscriptionManager transcriptionManager; // Reference to the BackAndForthTranscriptionManager
    [SerializeField] private string personaType;
    [SerializeField] private GameObject questionButton;
    [SerializeField] private GameObject replayButton;
    // [SerializeField] private GameObject suggestionButton;
    // [SerializeField] private GameObject replaySuggestionButton;

    public string CurrentVoice { get; private set; }
    public List<string> allQuestions { get; private set; } = new List<string>();
    private Queue<string> questionQueue = new Queue<string>();
    private Queue<string> suggestionQueue = new Queue<string>();
    private string lastQuestionText; // track last question
    private string lastSuggestionText; // track last suggestion

    private bool isQuestionBeingProcessed = false;

    void Start()
    {
        communicationManager.OnAvatarDataReady.AddListener(HandleAvatarDataReceived);
        recordButton.onClick.AddListener(StartRecordingResponse);
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
        foreach (var suggestion in avatarData.Suggestions)
        {
            suggestionQueue.Enqueue(suggestion);
        }
        Debug.Log($"Data updated for {avatarData.Persona} with voice {CurrentVoice}. Total questions loaded: {questionQueue.Count}");
        Debug.Log($"Total suggestions loaded: {suggestionQueue.Count}");
    }

    public void ClearQuestions()
    {
        questionQueue.Clear();
        suggestionQueue.Clear();
        allQuestions.Clear();
        communicationManager.OnAvatarDataReady.AddListener(HandleAvatarDataReceived);
    }

    public void AskQuestion()
    {
        if (audioSource.isPlaying || isQuestionBeingProcessed || questionQueue.Count == 0)
        {
            Debug.Log("Waiting: Audio is still playing, question is being processed, or no questions are available.");
            return;
        }

        isQuestionBeingProcessed = true;
        string questionText = questionQueue.Dequeue(); // Get the next question
        lastQuestionText = questionText; // Update the last question

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

    public void ReplayLastQuestion()
    {
        if (audioSource.isPlaying || isQuestionBeingProcessed || string.IsNullOrEmpty(lastQuestionText))
        {
            Debug.Log("Waiting: Audio is still playing, question is being processed, or no last question is available.");
            return;
        }

        isQuestionBeingProcessed = true;

        string jsonData = JsonUtility.ToJson(new TTSRequestData
        {
            model = "tts-1",
            input = lastQuestionText,
            voice = CurrentVoice
        });

        StartCoroutine(tts.GetTTS(jsonData, clip =>
        {
            isQuestionBeingProcessed = false;
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("Playing audio for question: " + lastQuestionText);
            }
            else
            {
                Debug.LogError("Failed to load audio for question: " + lastQuestionText);
            }
        }));
    }

    // public void StartRecordingResponse()
    // {
    //     transcriptionManager.StartTimer();
    //     dictationService.Toggle(); // Start dictation
    //     buttonText.text = "Stop Recording";
    //     recordButton.onClick.RemoveAllListeners();
    //     recordButton.onClick.AddListener(StopRecordingResponse);
    // }

    // public void StopRecordingResponse()
    // {
    //     transcriptionManager.StopTimer();
    //     dictationService.Toggle(); // Stop dictation
    //     string relevantTranscript = transcriptionManager.GetRelevantTranscript();
    //     HandleTranscription(relevantTranscript);
    //     buttonText.text = "Start Recording";
    //     recordButton.onClick.RemoveAllListeners();
    //     recordButton.onClick.AddListener(StartRecordingResponse);
    // }

    public void StartRecordingResponse()
    {
        transcriptionManager.StartTimer();
        // audioRecorder.StartRecording();
        buttonText.text = "Stop Recording";
        recordButton.onClick.RemoveAllListeners();
        recordButton.onClick.AddListener(StopRecordingResponse);
    }

    public void StopRecordingResponse()
    {
        transcriptionManager.StopTimer();
        // audioRecorder.StopRecording();
        string relevantTranscript = transcriptionManager.GetRelevantTranscript();
        HandleTranscription(relevantTranscript);
        buttonText.text = "Start Recording";
        recordButton.onClick.RemoveAllListeners();
        recordButton.onClick.AddListener(StartRecordingResponse);
    }

    public void HandleTranscription(string transcription)
    {
        string combinedText = $"Q: {lastQuestionText}\nA: {transcription}";
        GenerateResponse(combinedText);
    }

    private void GenerateResponse(string combinedText)
    {
        // Append the user's response to the conversation history
        conversationHistory.Add($"Q: {lastQuestionText}\nA: {combinedText}");

        // Combine the conversation history for context
        string conversationContext = string.Join("\n", conversationHistory);

        // Check if the conversation is during the Q/A portion
        bool isQA = conversationHistory.Any(entry => entry.Contains("###Transcript end###"));

        string systemPrompt;

        if (personaType == "Professional")
        {
            systemPrompt = "Pretend you are a professional well-versed in the topic. You are an AI that generates concise and insightful responses based on user input and context. Make sure to keep your response concise and directly address the user's answer, staying on topic.";
        }
        else if (personaType == "Novice")
        {
            systemPrompt = "Pretend you are a novice who is not well-versed in the topic. You are an AI that generates concise and insightful responses based on user input and context. Make sure to keep your response concise and directly address the user's answer, staying on topic.";
        }
        else
        {
            systemPrompt = "You are an AI that generates concise and insightful responses based on user input and context. Make sure to keep your response concise and directly address the user's answer, staying on topic.";
        }

        if (isQA)
        {
            systemPrompt += " This is during the Q/A portion of the talk.";
        }

        communicationManager.GenerateResponse(systemPrompt, conversationContext, response =>
        {
            // Append the AI's response to the conversation history
            conversationHistory.Add($"AI: {response}");

            PlayResponse(response);
        });
    }
    // private void GenerateResponse(string combinedText)
    // {
    //     // Append the user's response to the conversation history
    //     conversationHistory.Add($"Q: {lastQuestionText}\nA: {combinedText}");

    //     // Combine the conversation history for context
    //     string conversationContext = string.Join("\n", conversationHistory);

    //     string systemPrompt;

    //     if (personaType == "Professional")
    //     {
    //         systemPrompt = "Pretend you are a professional well-versed in the topic. You are an AI that generates concise and insightful responses based on user input and context. Make sure to keep your response concise and directly address the user's answer, staying on topic.";
    //     }
    //     else if (personaType == "Novice")
    //     {
    //         systemPrompt = "Pretend you are a novice who is not well-versed in the topic. You are an AI that generates concise and insightful responses based on user input and context. Make sure to keep your response concise and directly address the user's answer, staying on topic.";
    //     }
    //     else
    //     {
    //         systemPrompt = "You are an AI that generates concise and insightful responses based on user input and context. Make sure to keep your response concise and directly address the user's answer, staying on topic.";
    //     }

    //     communicationManager.GenerateResponse(systemPrompt, conversationContext, response =>
    //     {
    //         // Append the AI's response to the conversation history
    //         conversationHistory.Add($"AI: {response}");

    //         PlayResponse(response);
    //     });
    // }

    // private void GenerateResponse(string combinedText)
    // {
    //     string systemPrompt;

    //     if (personaType == "Professional")
    //     {
    //         systemPrompt = "Pretend you are a professional well-versed in the topic. You are an AI that generates concise and insightful responses based on user input and context. Make sure to keep your response concise and directly address the user's answer, staying on topic.";
    //     }
    //     else if (personaType == "Novice")
    //     {
    //         systemPrompt = "Pretend you are a novice who is not well-versed in the topic. You are an AI that generates concise and insightful responses based on user input and context. Make sure to keep your response concise and directly address the user's answer, staying on topic.";
    //     }
    //     else
    //     {
    //         systemPrompt = "You are an AI that generates concise and insightful responses based on user input and context. Make sure to keep your response concise and directly address the user's answer, staying on topic.";
    //     }

    //     communicationManager.GenerateResponse(systemPrompt, combinedText, response =>
    //     {
    //         PlayResponse(response);
    //     });
    // }

    // private void GenerateResponse(string combinedText)
    // {
    //     string systemPrompt = "You are an AI that generates insightful responses based on user input and context.";

    //     communicationManager.GenerateResponse(systemPrompt, combinedText, response =>
    //     {
    //         // Limit response to a certain number of characters (approximate token count)
    //         int maxLength = 200; // Adjust as needed to approximate 50 tokens
    //         if (response.Length > maxLength)
    //         {
    //             response = response.Substring(0, maxLength) + "...";
    //         }

    //         PlayResponse(response);
    //     });
    // }
    // private void GenerateResponse(string combinedText)
    // {
    //     string systemPrompt = "You are an AI that generates insightful responses based on user input and context.";

    //     communicationManager.GenerateResponse(systemPrompt, combinedText, response =>
    //     {
    //         PlayResponse(response);
    //     });
    // }

    private void PlayResponse(string responseText)
    {
        string jsonData = JsonUtility.ToJson(new TTSRequestData
        {
            model = "tts-1",
            input = responseText,
            voice = CurrentVoice
        });

        StartCoroutine(tts.GetTTS(jsonData, clip =>
        {
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("Playing AI response: " + responseText);
            }
            else
            {
                Debug.LogError("Failed to load audio for AI response: " + responseText);
            }
        }));
    }

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

    public void ShowReplayButton()
    {
        replayButton.SetActive(true);
    }

    public void HideReplayButton()
    {
        replayButton.SetActive(false);
    }

    public void ShowSuggestionButton()
    {
        // suggestionButton.SetActive(true);
    }
    
    public void HideSuggestionButton()
    {
        // suggestionButton.SetActive(false);
    }
    
    public void ShowReplaySuggestionButton()
    {
        // replaySuggestionButton.SetActive(true);
    }
    
    public void HideReplaySuggestionButton()
    {
        // replaySuggestionButton.SetActive(false);
    }
}