using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private GameObject startResponseButton; // Button to start response
    [SerializeField] private TMP_Text stopResponseButtonText; // Text on the stop response button
    [SerializeField] private Button stopResponseButton; // Button to stop response

    public TranscriptionLogger transcriptionLogger; // Reference to the TranscriptLogger
    public string CurrentVoice { get; private set; }
    public List<string> allQuestions { get; private set; } = new List<string>();
    private Stack<string> questionQueue = new();
    private Stack<string> suggestionQueue = new();
    private string lastQuestionText; // track last question
    private string lastSuggestionText; // track last suggestion
    private string conversationHistory = ""; // track conversation history

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
        conversationHistory = "";
        hideAllButtons();
    }

    public void hideAllButtons()
    {
        HideQuestionButton();
        HideReplayButton();
        HideStartResponseButton();
        HideStopResponseButton();
    }

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
        initConversation(); // Initialize conversation history

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

        // Autopause
        if (record.GetRecording())
        {
            startButton.onClick.Invoke();
            Debug.Log("Paused");
        }

        // Hide the button when no more questions
        if (questionQueue.Count == 0)
        {
            questionButton.SetActive(false);
        }
    }

    public void initConversation()
    {
        conversationHistory = "";
        conversationHistory += $"You are a {personaType} on the topic. You have been listening to the user give a talk. During the talk or during the Q&A session, you asked the user a question related to their talk. The user has now responded to your question. You may ask a follow-up question or provide a comment. Ensure your responses are concise, directly address the user's answer, and stay on topic. Limit the number of responses you give to allow time for others to ask questions.";
        conversationHistory += "\n";
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
        if (record.GetRecording())
        {
            startButton.onClick.Invoke();
            Debug.Log("Paused");
        }
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
        if (record.GetRecording())
        {
            startButton.onClick.Invoke();
            Debug.Log("Paused");
        }
    }

    private void PlayResponse(string responseText)
    {
        isQuestionBeingProcessed = true;
        lastQuestionText = responseText;

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
                isQuestionBeingProcessed = false;
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

    public void StartResponse()
    {
        HideStartResponseButton();
        HideQuestionButton();
        HideReplayButton();
        ShowStopResponseButton();
        if (audioSource.isPlaying || isQuestionBeingProcessed)
        {
            Debug.Log("Waiting: Audio is still playing or question is being processed.");
            return;
        }
        transcriptionLogger.SetResponseActive(true);
    }

    public void StopResponse()
    {
        transcriptionLogger.SetResponseActive(false);
        stopResponseButton.interactable = false;
        stopResponseButtonText.text = "Thinking...";

        string responseTranscript = string.Join(" ", transcriptionLogger.GetResponseList());

        // Update conversation history
        conversationHistory += $"{personaType}: {lastQuestionText}\n";
        conversationHistory += $"User: {responseTranscript}\n";

        // Clear the response transcript in TranscriptionLogger
        transcriptionLogger.ResetResponseTranscript();

        MakeApiCall(conversationHistory);
    }

    private void MakeApiCall(string data)
    {
        Debug.Log("Making API call with data: " + data);
        communicationManager.GenerateResponse(data, response =>
        {
            // Append the AI's response to the conversation history
            conversationHistory += $"AI: {response}\n";

            PlayResponse(response);
            ShowQuestionButton();
            ShowReplayButton();
            ShowStartResponseButton();
            HideStopResponseButton();
        },
        error =>
        {
            // Handle error by playing a default response
            Debug.LogError(error);
            string defaultResponse = "No further questions";
            conversationHistory += $"AI: {defaultResponse}\n";
            PlayResponse(defaultResponse);
            ShowQuestionButton();
            ShowReplayButton();
            ShowStartResponseButton();
            HideStopResponseButton();
        });
    }

    [Serializable]
    public class TTSRequestData
    {
        public string model;
        public string input;
        public string voice;
    }

    public void ShowStartResponseButton()
    {
        startResponseButton.gameObject.SetActive(true);
    }

    public void HideStartResponseButton()
    {
        startResponseButton.gameObject.SetActive(false);
    }

    public void ShowStopResponseButton()
    {
        stopResponseButton.gameObject.SetActive(true);
        stopResponseButton.interactable = true;
        stopResponseButtonText.text = "Stop Response";
    }

    public void HideStopResponseButton()
    {
        stopResponseButton.gameObject.SetActive(false);
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