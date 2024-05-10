using System;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;
using UnityEngine.Events;
using System.Text.RegularExpressions;

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
    private OpenAIApi openAI = new OpenAIApi(OpenAIConfig.ApiKey);
    private List<ChatMessage> messages = new List<ChatMessage>();
    private static readonly List<string> voices = new List<string> { "alloy", "echo", "fable", "onyx", "nova", "shimmer" };

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
                Model = "gpt-4-turbo"
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

    private void ProcessResponse(ChatMessage response)
    {
        Debug.Log(response.Content);
        OnResponsesReady.Invoke();

        // Process Professional Data
        string professionalContent = ExtractContent(response.Content, "Professional:", "Novice:");
        Debug.Log($"Professional content: {professionalContent}");
        var professionalSections = ParseSections(professionalContent);
        Debug.Log($"Professional sections: {string.Join(", ", professionalSections.Keys)}");
        var professionalSuggestions = ParseSuggestions(professionalContent);
        Debug.Log($"Professional suggestions: {string.Join(", ", professionalSuggestions)}");
        AvatarData professionalData = new AvatarData
        {
            Persona = "Professional",
            Voice = "alloy",
            Sections = professionalSections,
            Suggestions = professionalSuggestions
        };
        DebugAvatarData(professionalData);
        OnAvatarDataReady.Invoke(professionalData);

        // Process Novice Data
        string noviceContent = ExtractContent(response.Content, "Novice:", "EndOfContent");
        Debug.Log($"Novice content: {noviceContent}");
        var noviceSections = ParseSections(noviceContent);
        Debug.Log($"Novice sections: {string.Join(", ", noviceSections.Keys)}");
        var noviceSuggestions = ParseSuggestions(noviceContent);
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
            if (collecting && !string.IsNullOrEmpty(line.Trim()))
            {
                suggestions.Add(line.Substring(3).Trim()); // Remove numbering like '1. ' and trim
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
}
