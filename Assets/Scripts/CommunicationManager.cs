using System;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;
using UnityEngine.Events;

public class CommunicationManager : MonoBehaviour
{
    public UnityEvent<AvatarData> OnAvatarDataReady;
    public UnityEvent OnResponsesReady;

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
    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();
    private static readonly List<string> voices = new List<string> { "alloy", "echo", "fable", "onyx", "nova", "shimmer" };

    [SerializeField] private GameObject professionalQuestionGameObject;
    [SerializeField] private GameObject noviceQuestionGameObject;
    void Start()
    {
        transcriptionProcessor.onTranscriptionReady.AddListener(AskChatGPT);
    }

    private string GenerateCombinedText()
    {
        string promptText = @"
        Given the full transcription for a presentation, do the following:
        1. Read the full transcript and divide it into separate sections (e.g., motivation, overview, related work).
        2. Pretend you are a professional well-versed in the topic.
        3. Ask questions targeted for each section. Make sure to include at least one question for each section. Make sure the questions fit your character. Make sure each question is no more than two sentences. Do not contain the word 'Novice' in the question.
        4. List at least two areas of improvement in the presentation. Make sure the suggestions fit your character. Make sure each suggestion is no more than two sentences. Do not contain the word 'Novice' in the suggestion.
        5. Pretend you are a novice who is not well-versed in the topic.
        6. Ask questions targeted for each section. Make sure to include at least one question for each section. Make sure the questions fit your character. Make sure each question is no more than two sentences. Do not contain the word 'Professional' in the question.
        7. List at least two areas of improvement in the presentation. Make sure the suggestions fit your character. Make sure each suggestion is no more than two sentences. Do not contain the word 'Professional' in the suggestion.
        8. Output your answer in the following format:
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
        '''";

        string speechLogText = "";

        List<string> list = transcriptionLogger.GetTranscriptionList();
        for (int i = 0; i < list.Count; i++) {
            speechLogText += list[i];
        }

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
                Model = "gpt-4-turbo"
            };

            var response = await openAI.CreateChatCompletion(request);
            if (response.Choices != null && response.Choices.Count > 0)
            {
                ProcessResponse(response.Choices[0].Message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while processing chat response: {ex.Message}");
        }
    }

    private void ProcessResponse(ChatMessage response)
    {
        Debug.Log(response.Content);
        OnResponsesReady.Invoke();

        // Process Professional Data
        string professionalContent = ExtractContent(response.Content, "Professional:", "Novice:");
        AvatarData professionalData = new AvatarData
        {
            Persona = "Professional",
            Voice = "alloy",
            Sections = ParseSections(professionalContent),
            Suggestions = ParseSuggestions(professionalContent)
        };
        DebugAvatarData(professionalData);
        OnAvatarDataReady.Invoke(professionalData);

        // Show professional question when available.
        professionalQuestionGameObject.SetActive(true);

        // Process Novice Data
        string noviceContent = ExtractContent(response.Content, "Novice:", "EndOfContent");
        AvatarData noviceData = new AvatarData
        {
            Persona = "Novice",
            Voice = "onyx",
            Sections = ParseSections(noviceContent),
            Suggestions = ParseSuggestions(noviceContent)
        };
        DebugAvatarData(noviceData);
        OnAvatarDataReady.Invoke(noviceData);

        // Show novice question button when available.
        noviceQuestionGameObject.SetActive(true);
    }

    private string ExtractContent(string content, string startKeyword, string endKeyword)
    {
        int startIndex = content.IndexOf(startKeyword);
        if (startIndex == -1)
        {
            Debug.LogError($"Start keyword '{startKeyword}' not found.");
            return string.Empty;
        }

        startIndex += startKeyword.Length;
        int endIndex = content.IndexOf(endKeyword, startIndex);
        if (endIndex == -1)
        {
            Debug.LogWarning($"End keyword '{endKeyword}' not found after start keyword. Assuming end of content.");
            endIndex = content.Length; // If not found, assume end of content
        }

        return content.Substring(startIndex, endIndex - startIndex).Trim();
    }

    private Dictionary<string, List<string>> ParseSections(string responseData)
    {
        var sections = new Dictionary<string, List<string>>();
        var lines = responseData.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("Section"))
            {
                string sectionTitle = lines[i].Substring(lines[i].IndexOf(' ') + 1).Trim();
                sections[sectionTitle] = new List<string>
                {
                    lines[++i].Substring(3).Trim(), // Increment i to skip to the question, and substring to remove numbering like '1. '
                    lines[++i].Substring(3).Trim()  // Same here for the second question
                };
            }
            else if (lines[i].StartsWith("Suggestions:"))
            {
                // Once we hit "Suggestions:", we stop processing as no more sections are expected.
                break;
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
                continue; // Move to next iteration to skip "Suggestions:" line itself
            }
            if (collecting && !string.IsNullOrEmpty(line.Trim()))
            {
                suggestions.Add(line.Substring(3).Trim()); // Remove the number and trim the string
            }
        }

        return suggestions;
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
}
