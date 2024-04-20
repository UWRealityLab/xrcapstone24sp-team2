using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private OpenAITTS tts;
    [SerializeField] private AudioSource audioSource;
    private Queue<Question> questionQueue = new Queue<Question>();

    [System.Serializable]
    public class QuestionsList
    {
        public List<Question> questions;
    }

    [System.Serializable]
    public class Question
    {
        public int number;
        public string text;
        public string voice;
    }

    [System.Serializable]
    public class TTSRequestData
    {
        public string model;
        public string input;
        public string voice;
    }

    void Start()
    {
        LoadQuestions();
    }

    private void LoadQuestions()
    {
        string filePath = "Data/questions.json";
        string fullPath = Path.Combine(Application.dataPath, filePath);
        if (File.Exists(fullPath))
        {
            string fileContents = File.ReadAllText(fullPath);
            QuestionsList loadedQuestions = JsonUtility.FromJson<QuestionsList>(fileContents);
            foreach (Question question in loadedQuestions.questions)
            {
                questionQueue.Enqueue(question); // Enqueue each question
            }
            Debug.Log("Loaded " + loadedQuestions.questions.Count + " questions and initialized the queue.");
        }
        else
        {
            Debug.LogError("Cannot find the questions file at: " + fullPath);
        }
    }

    public void AskNextQuestion()
    {
<<<<<<< HEAD
=======
        // Check if the audio source is currently playing
        if (audioSource.isPlaying)
        {
            Debug.Log("Audio is still playing. Waiting until the current audio finishes.");
            return; // Exit the method if audio is still playing
        }

>>>>>>> temp
        if (questionQueue.Count > 0)
        {
            Question question = questionQueue.Dequeue(); // Dequeue the next question
            PlayQuestion(question);
        }
        else
        {
            Debug.Log("No more questions available.");
        }
    }

    private void PlayQuestion(Question question)
    {
        string jsonData = JsonUtility.ToJson(new TTSRequestData
        {
            model = "tts-1",
            input = question.text,
            voice = question.voice
        });

        Debug.Log("Sending JSON for TTS: " + jsonData);
        StartCoroutine(tts.GetTTS(jsonData, clip =>
        {
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("Playing audio for question: " + question.text);
            }
            else
            {
                Debug.LogError("Failed to load audio for question: " + question.text);
            }
        }));
    }
}