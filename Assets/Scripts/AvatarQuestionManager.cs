using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarQuestionManager : MonoBehaviour
{
    [SerializeField] private OutputAudioRecorder audioRecorder;
    [SerializeField] private Button recordButton;
    [SerializeField] private Text buttonText;
    [SerializeField] private CommunicationManager communicationManager;
    [SerializeField] private OpenAITTS tts;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string personaType;
    [SerializeField] private GameObject questionButton;
    [SerializeField] private GameObject replayButton;
    [SerializeField] private GameObject suggestionButton;
    [SerializeField] private GameObject replaySuggestionButton;

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
    public void StartRecordingResponse()
    {
        audioRecorder.StartRecording();
        buttonText.text = "Stop Recording";
        recordButton.onClick.RemoveAllListeners();
        recordButton.onClick.AddListener(StopRecordingResponse);
    }

    public void StopRecordingResponse()
    {
        audioRecorder.StopRecording();
        audioRecorder.SendWavToOpenAI(audioRecorder.FileName, HandleTranscription);
        buttonText.text = "Start Recording";
        recordButton.onClick.RemoveAllListeners();
        recordButton.onClick.AddListener(StartRecordingResponse);
    }

    public void HandleTranscription(string transcription)
    {
        string combinedText = $"Q: {lastQuestionText}\nA: {transcription}";
        GenerateResponse(combinedText);
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

    public void AskSuggestion()
    {
        if (audioSource.isPlaying || isQuestionBeingProcessed || suggestionQueue.Count == 0)
        {
            Debug.Log("Waiting: Audio is still playing, question is being processed, or no suggestions are available.");
            return;
        }

        isQuestionBeingProcessed = true;
        string suggestionText = suggestionQueue.Dequeue();
        lastSuggestionText = suggestionText;

        string jsonData = JsonUtility.ToJson(new TTSRequestData
        {
            model = "tts-1",
            input = suggestionText,
            voice = CurrentVoice
        });

        StartCoroutine(tts.GetTTS(jsonData, clip =>
        {
            isQuestionBeingProcessed = false;
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("Playing audio for suggestion: " + suggestionText);
            }
            else
            {
                Debug.LogError("Failed to load audio for suggestion: " + suggestionText);
            }
        }));
    }

    public void ReplayLastSuggestion()
    {
        if (audioSource.isPlaying || isQuestionBeingProcessed || string.IsNullOrEmpty(lastSuggestionText))
        {
            Debug.Log("Waiting: Audio is still playing, question is being processed, or no last suggestion is available.");
            return;
        }

        isQuestionBeingProcessed = true;

        string jsonData = JsonUtility.ToJson(new TTSRequestData
        {
            model = "tts-1",
            input = lastSuggestionText,
            voice = CurrentVoice
        });

        StartCoroutine(tts.GetTTS(jsonData, clip =>
        {
            isQuestionBeingProcessed = false;
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("Playing audio for suggestion: " + lastSuggestionText);
            }
            else
            {
                Debug.LogError("Failed to load audio for suggestion: " + lastSuggestionText);
            }
        }));
    }

    private void GenerateResponse(string combinedText)
    {
        string systemPrompt = "You are an AI that generates insightful responses based on user input and context.";

        communicationManager.GenerateResponse(systemPrompt, combinedText, response =>
        {
            PlayResponse(response);
        });
    }

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
        suggestionButton.SetActive(true);
    }

    public void HideSuggestionButton()
    {
        suggestionButton.SetActive(false);
    }

    public void ShowReplaySuggestionButton()
    {
        replaySuggestionButton.SetActive(true);
    }

    public void HideReplaySuggestionButton()
    {
        replaySuggestionButton.SetActive(false);
    }
}