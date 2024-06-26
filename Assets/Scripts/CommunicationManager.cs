using System;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.UI;
using UI;

public class CommunicationManager : MonoBehaviour
{
    private Dictionary<string, HashSet<string>> previousQuestions = new Dictionary<string, HashSet<string>>();
    public UnityEvent<AvatarData> OnAvatarDataReady;
    public UnityEvent OnResponsesReady;
    [SerializeField]
    public GradeDisplay gradeDisplay;

    [Serializable]
    public class AvatarData
    {
        public string Persona;
        public string Voice { get; set; }
        public Dictionary<string, List<string>> Sections { get; set; }
        public List<string> Suggestions { get; set; }
    }

    public TranscriptionLogger transcriptionLogger;
    public TranscriptionProcessor transcriptionProcessor;
    private OpenAIApi openAI = new OpenAIApi(OpenAIConfig.ApiKey);
    private List<ChatMessage> messages = new List<ChatMessage>();
    private static readonly List<string> voices = new List<string> { "alloy", "echo", "fable", "onyx", "nova", "shimmer" };
    private static readonly string[] rubricTitles = { "Engagement", "Organization", "Storytelling", "Filler words", "Articulation"};
    public (string, int, string)[] grade;
    public List<string> proSuggestions;


    private float chatGPTRequestInterval = 30.0f;
    private float lastChatGPTRequestTime;
    public TimerController timerController;

    private void Start()
    {
        lastChatGPTRequestTime = timerController.GetElapsedTimeInSeconds();
    }

    private void Update()
    {
        if (timerController.GetElapsedTimeInSeconds() - lastChatGPTRequestTime >= chatGPTRequestInterval)
        {
            AskChatGPT();
            lastChatGPTRequestTime = timerController.GetElapsedTimeInSeconds();
        }
    }

    private string GenerateCombinedText()
    {
        string promptText = @"
        Given the full transcription for a presentation, do the following:

        1. Read the full transcript and divide it into separate sections (e.g., motivation, overview, related work).
        2. Pretend you are a professional well-versed in the topic.
        3. Ask questions targeted for each section. Make sure to include at least one question for each section. Make sure the questions fit your character. Make sure each question is no more than two sentences. Do not contain the word 'Novice' in the question. Avoid repeating these questions: " + GetPreviousQuestionsText("Professional") + @"
        4. List at least two areas of improvement in the presentation. Make sure the suggestions fit your character. Make sure each suggestion is in one short sentence. Do not contain the word 'Novice' in the suggestion.
        5. Pretend you are a novice who is not well-versed in the topic.
        6. Ask questions targeted for each section. Make sure to include at least one question for each section. Make sure the questions fit your character. Make sure each question is no more than two sentences. Do not contain the word 'Professional' in the question. Avoid repeating these questions: " + GetPreviousQuestionsText("Novice") + @"
        7. List at least two areas of improvement in the presentation. Make sure the suggestions fit your character. Make sure each suggestion is in one short sentence. Do not contain the word 'Professional' in the suggestion.
        8. Give the user a grade based on the following rubric with a scale 0-10.
        - How well it keeps the audience engaged.
        - How organized the speech is.
        - The storytelling of the speech.
        - The amount of filler words and stutters in the speech.
        - The articulation of the speech.
        9. Output your answer in the following format and in plain text (e.g., no Markdown, no HTML):
        '''
        Professional:
        Questions:
        Section <section name>
        1. ...
        2. ...
        Section <section name>
        1. ...
        2. ...
        Suggestions:
        1. ...
        2. ...
        Novice:
        Questions:
        Section <section name>
        1. ...
        2. ...
        Section <section name>
        1. ...
        2. ...
        Suggestions:
        1. ...
        2. ...
        Rubric:
        <rubric name>+<grade>+<feedback>
        <rubric name>+<grade>+<feedback>
        '''";
        string speechLogText = "";

        // transcription without timestamps
        List<string> list = transcriptionLogger.GetTranscriptionList();
        foreach (string transcription in list)
        {
            speechLogText += transcription + " ";
        }

        // timestamped transcriptione
        // example format:
        // 00:01 - Good morning, everyone.
        // 00:10 - Today, we are going to discuss the latest trends in technology.
        // List<(string, string)> timeList = transcriptionLogger.GetTranscriptionTimeList();
        // foreach (var (time, transcription) in timeList)
        // {
        //     speechLogText += $"{time} - {transcription}\n";
        // }

        Debug.Log("Speech Text: " + speechLogText);

        return promptText + "\n" + speechLogText;
    }

    public async void AskChatGPT()
    {
        Debug.Log("ChatGPT request initiated.");
        try
        {
            string combinedText = GenerateCombinedText();
            messages.Add(new ChatMessage { Content = combinedText, Role = "user" });

            CreateChatCompletionRequest request = new CreateChatCompletionRequest
            {
                Messages = messages,
                Model = "gpt-4o"
            };

            var response = await openAI.CreateChatCompletion(request);
            if (response.Choices != null && response.Choices.Count > 0)
            {
                Debug.Log(response.Choices[0].Message);
                ProcessResponse(response.Choices[0].Message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while processing chat response: {ex.Message}");
        }
    }
    public async void GenerateResponse(string data, Action<string> callback, Action<string> errorCallback)
    {
        Debug.Log("OpenAI request initiated.");
        try
        {
            // Create messages list with system and user messages
            var responseMessages = new List<ChatMessage>
            {
                new ChatMessage { Content = data, Role = "user" }
            };

            // Create the request for chat completion
            CreateChatCompletionRequest request = new CreateChatCompletionRequest
            {
                Messages = responseMessages,
                Model = "gpt-4o"
            };

            // Send the request to OpenAI
            var response = await openAI.CreateChatCompletion(request);

            // Check if response is valid and process the first choice
            if (response.Choices != null && response.Choices.Count > 0)
            {
                Debug.Log(response.Choices[0].Message.Content);
                callback(response.Choices[0].Message.Content);
            }
            else
            {
                string errorMessage = "No choices were returned by the API.";
                Debug.LogError(errorMessage);
                errorCallback(errorMessage);
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error while processing chat response: {ex.Message}";
            Debug.LogError(errorMessage);
            errorCallback(errorMessage);
        }
    }

    private void ProcessResponse(ChatMessage response)
    {
        Debug.Log(response.Content);
        OnResponsesReady.Invoke();

        // Process Professional Data
        string professionalContent = ExtractContent(response.Content, "Professional:", "Novice:");
        var professionalSections = ParseSections(professionalContent);
        StoreQuestions("Professional", professionalSections);
        var professionalSuggestions = ParseSuggestions(professionalContent);

        Debug.Log($"Professional content: {professionalContent}");
        Debug.Log($"Professional sections: {string.Join(", ", professionalSections.Keys)}");
        Debug.Log($"Professional suggestions: {string.Join(", ", professionalSuggestions)}");
        AvatarData professionalData = new AvatarData
        {
            Persona = "Professional",
            Voice = "alloy",
            Sections = professionalSections,
            Suggestions = professionalSuggestions
        };
        proSuggestions = professionalSuggestions;
        DebugAvatarData(professionalData);
        OnAvatarDataReady.Invoke(professionalData);

        // Process Novice Data
        string noviceContent = ExtractContent(response.Content, "Novice:", "Rubric:");
        var noviceSections = ParseSections(noviceContent);
        StoreQuestions("Novice", noviceSections);
        var noviceSuggestions = ParseSuggestions(noviceContent);

        Debug.Log($"Novice content: {noviceContent}");
        Debug.Log($"Novice sections: {string.Join(", ", noviceSections.Keys)}");
        Debug.Log($"Novice suggestions: {string.Join(", ", noviceSuggestions)}");
        AvatarData noviceData = new AvatarData
        {
            Persona = "Novice",
            Voice = "onyx",
            Sections = noviceSections,
            Suggestions = noviceSuggestions
        };
        DebugAvatarData(noviceData);
        OnAvatarDataReady.Invoke(noviceData);

        // Process Rubric
        string rubricContent = ExtractContent(response.Content, "Rubric", "EndOfContent");
        Debug.Log($"RubricContent content: {rubricContent}");
        grade = ParseRubric(rubricContent);

    }

    private string ExtractContent(string content, string startKeyword, string endKeyword)
    {
        // Adjust regex to optionally include endKeyword if it exists
        var regex = new Regex($"{Regex.Escape(startKeyword)}(.*?)(?:{Regex.Escape(endKeyword)}|$)", RegexOptions.Singleline);
        var match = regex.Match(content);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
        else
        {
            Debug.LogWarning($"Start keyword '{startKeyword}' not found. Returning full content.");
            return content;
        }
    }

    private Dictionary<string, List<string>> ParseSections(string responseData)
    {
        var sections = new Dictionary<string, List<string>>();
        var lines = responseData.Split('\n');
        bool inSection = false;
        string currentSection = "";

        foreach (var line in lines)
        {
            if (line.StartsWith("Section"))
            {
                if (inSection && sections.ContainsKey(currentSection))
                {
                    // This handles the switch to a new section
                    inSection = false;
                }

                currentSection = line.Substring(line.IndexOf(' ') + 1).Trim();
                sections[currentSection] = new List<string>();
                inSection = true;
            }
            else if (inSection && !string.IsNullOrEmpty(line.Trim()))
            {
                sections[currentSection].Add(line.Substring(3).Trim()); // Assume line starts with '1. ' or similar
            }
            else if (string.IsNullOrEmpty(line.Trim()) || line.StartsWith("Suggestions:"))
            {
                // Handle blank line or end of section signal by Suggestions
                inSection = false;
            }
        }

        return sections;
    }

    private List<string> ParseSuggestions(string responseData)
    {
        List<string> suggestions = new List<string>();
        bool collecting = false;

        foreach (string line in responseData.Split('\n'))
        {
            if (line.StartsWith("Suggestions:"))
            {
                collecting = true;
                continue; // Skip the "Suggestions:" line itself
            }
            if (collecting)
            {
                string suggestion = line.Substring(3).Trim(); // Remove numbering like '1. ' and trim
                if (!string.IsNullOrEmpty(suggestion))
                {
                    suggestions.Add(suggestion);
                }
            }
        }

        return suggestions;
    }

    public void ClearMessages()
    {
        messages.Clear();
    }

    private void DebugAvatarData(AvatarData avatarData)
    {
        Debug.Log($"Avatar Data Prepared: Persona={avatarData.Persona}, Voice={avatarData.Voice}");
        Debug.Log("Sections:");
        foreach (var section in avatarData.Sections)
        {
            Debug.Log($"Section: {section.Key}");
            foreach (var question in section.Value)
            {
                Debug.Log($" - {question}");
            }
        }

        Debug.Log("Suggestions:");
        foreach (var suggestion in avatarData.Suggestions)
        {
            Debug.Log(suggestion);
        }

    }

    private (string, int, string)[] ParseRubric(string responseData)
    {
        int amountOfGrades = 5;
        var grades = new (string rubric, int grade, string feedback)[amountOfGrades];
        int index = 0;
        foreach (string line in responseData.Split('\n'))
        {
            string[] grade = line.Split('+');
            if (grade.Length == 3)
            {
                grades[index] = (rubricTitles[index], Int32.Parse(grade[1]), grade[2]);
                index++;
            }
        }
        return grades;
    }

    public (string, int, string)[] GetGrade()
    {
        return grade;
    }

    private void StoreQuestions(string persona, Dictionary<string, List<string>> sections)
    {
        if (!previousQuestions.ContainsKey(persona))
        {
            previousQuestions[persona] = new HashSet<string>();
        }

        foreach (var section in sections)
        {
            foreach (var question in section.Value)
            {
                previousQuestions[persona].Add(question);
            }
        }
    }

    private string GetPreviousQuestionsText(string persona)
    {
        if (!previousQuestions.ContainsKey(persona))
        {
            return string.Empty;
        }

        return string.Join(", ", previousQuestions[persona]);
    }

    public List<string> GetSuggestions()
    {
        return proSuggestions;
    }

    public void ResetLastGPTRequestTime()
    {
        lastChatGPTRequestTime = timerController.GetElapsedTimeInSeconds();
    }
}
