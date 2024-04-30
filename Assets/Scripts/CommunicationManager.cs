using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using OpenAI;
using System.Linq;

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

    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();
    private static readonly List<string> voices = new List<string> { "alloy", "echo", "fable", "onyx", "nova", "shimmer" };

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

        string speechLogText = @"
        Full transcription:
        We present deck talk a virtual reality app that enables users to practice giving talks in virtualized environments to an interactable AI powered audience.
        Presentation it's common in both academic and professional settings
        To refine the presentation people often practice giving talks in front of a wall or mirror
        Where are limited amount of feedback can be obtained
        How can we make practice talks more effective and closer to real life?
        We designed deck talk based on three core concepts.
        First the user should be able to choose from a diverse set of environments to give toxin
        Second
        We are just taking virtual avatars should be present in the scene.
        To mimic real audiences.
        Third the user should be able to interact with the audience via question answering and receive feedback about the presentation
        The company virtual speech has developed a similar application with virtual reality and AI.
        Including varied Workplace settings
        Virtual Interactable avatars. And AI generated ratings.
        However, virtual speech requires the conversion of presentation sites into PDFs
        And it costs $400 a year
        In contrast we allow users to bring the presentation into the virtual environment
        The web browser and are planning to offer the app for free by open sourcing the code.
        In addition our app enables users to improve their general presentation skills by practicing against AI generated presentation sides.";

    void Start()
    {
        AskChatGPT();
    }

    public async void AskChatGPT()
    {
        try
        {
            string combinedText = GenerateCombinedText();
            Debug.Log("Combined text from prompt and speech log: " + combinedText);

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
            Voice = voices[new System.Random().Next(voices.Count)],
            Sections = ParseSections(professionalContent),
            Suggestions = ParseSuggestions(professionalContent)
        };
        DebugAvatarData(professionalData);
        OnAvatarDataReady.Invoke(professionalData);

        // Process Novice Data
        string noviceContent = ExtractContent(response.Content, "Novice:", "EndOfContent");
        AvatarData noviceData = new AvatarData
        {
            Persona = "Novice",
            Voice = voices[new System.Random().Next(voices.Count)],
            Sections = ParseSections(noviceContent),
            Suggestions = ParseSuggestions(noviceContent)
        };
        DebugAvatarData(noviceData);
        OnAvatarDataReady.Invoke(noviceData);
    }

    private string ExtractContent(string content, string startKeyword, string endKeyword)
    {
        int startIndex = content.IndexOf(startKeyword);
        if (startIndex != -1)
        {
            startIndex += startKeyword.Length;
            int endIndex = content.IndexOf(endKeyword, startIndex);
            endIndex = endIndex == -1 ? content.Length : endIndex;
            return content.Substring(startIndex, endIndex - startIndex).Trim();
        }
        return string.Empty;
    }

    private string GenerateCombinedText()
    {
        return promptText + "\n" + speechLogText;
    }

    private Dictionary<string, List<string>> ParseSections(string responseData)
    {
        var sections = new Dictionary<string, List<string>>();
        string[] lines = responseData.Split('\n');
        string currentSection = null;

        foreach (string line in lines)
        {
            if (line.StartsWith("Section"))
            {
                currentSection = line.Substring(line.IndexOf(" ") + 1).Trim();
                sections[currentSection] = new List<string>();
            }
            else if (currentSection != null && !string.IsNullOrEmpty(line.Trim()))
            {
                string cleanedLine = line.Trim().Split(new char[] {' '}, 2).LastOrDefault()?.Trim(); // Split and remove the numbering
                if (!string.IsNullOrEmpty(cleanedLine))
                    sections[currentSection].Add(cleanedLine);
            }
        }

        return sections;
    }

    private List<string> ParseSuggestions(string responseData)
    {
        List<string> suggestions = new List<string>();
        bool collecting = false;
        bool skipNextLine = false; // To skip the line immediately after "Suggestions:"

        foreach (string line in responseData.Split('\n'))
        {
            if (line.StartsWith("Suggestions:"))
            {
                collecting = true;
                skipNextLine = true;
                continue;
            }
            if (collecting && skipNextLine)
            {
                skipNextLine = false; // Skip this line and reset
                continue;
            }
            if (collecting && !string.IsNullOrEmpty(line.Trim()))
            {
                suggestions.Add(line.Trim());
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
