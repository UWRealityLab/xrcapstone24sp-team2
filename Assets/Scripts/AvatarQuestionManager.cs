using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarQuestionManager : MonoBehaviour
{
    [SerializeField] private CommunicationManager communicationManager; // Reference to the CommunicationManager
    [SerializeField] private Recording record; // Reference to the Recording Script
    [SerializeField] private Button startButton;
    [SerializeField] private OpenAITTS tts;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private string personaType;
    [SerializeField] private GameObject questionButton;
    [SerializeField] private GameObject replayButton;
    // [SerializeField] private GameObject suggestionButton;
    // [SerializeField] private GameObject replaySuggestionButton;

    public string CurrentVoice { get; private set; }
    public List<string> allQuestions { get; private set; } = new List<string>();
    private Stack<string> questionQueue = new();
    private Stack<string> suggestionQueue = new();
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
                questionQueue.Push(question);
            }
        }
        foreach (var suggestion in avatarData.Suggestions)
        {
            suggestionQueue.Push(suggestion);
        }
        Debug.Log($"Data updated for {avatarData.Persona} with voice {CurrentVoice}. Total questions loaded: {questionQueue.Count}");
        Debug.Log($"Total suggestions loaded: {suggestionQueue.Count}");
    }

    public void ClearQuestions()
    {
        questionQueue.Clear();
        suggestionQueue.Clear();
        allQuestions.Clear();
        lastQuestionText = null;
        communicationManager.OnAvatarDataReady.AddListener(HandleAvatarDataReceived);
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
        string questionText = questionQueue.Pop(); // Get the next question
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
        if (record.GetRecording())
        {
            startButton.onClick.Invoke();
            Debug.Log("Paused");
        }
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
        string suggestionText = suggestionQueue.Pop();
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