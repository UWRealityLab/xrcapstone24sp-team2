using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class AvatarCreator
{
    // Define a list of voices available for use.
    private static readonly List<string> voices = new List<string> { "alloy", "echo", "fable", "onyx", "nova", "shimmer" };

    // Declare the event using the public AvatarData class.
    public static event Action<AvatarData> OnAvatarDataCreated;

    // AvatarData class defined as public to be accessible by other scripts.
    public class AvatarData
    {
        public string Persona { get; set; }
        public string Voice { get; set; }
        public Dictionary<string, List<string>> Sections { get; set; }
        public List<string> Suggestions { get; set; }
    }

    public static void Main()
    {
        // Process response files and invoke the event with AvatarData.
        string response1Path = "Assets/TextFiles/Response1.txt";
        string response2Path = "Assets/TextFiles/Response2.txt";

        AvatarData response1Data = ProcessResponseFile(response1Path, "professional");
        AvatarData response2Data = ProcessResponseFile(response2Path, "novice");

        SaveAsJson(response1Data, "Professional.json");
        SaveAsJson(response2Data, "Novice.json");

        // Emit event with the AvatarData.
        OnAvatarDataCreated?.Invoke(response1Data);
        OnAvatarDataCreated?.Invoke(response2Data);
    }

    private static AvatarData ProcessResponseFile(string filePath, string persona)
    {
        var sections = new Dictionary<string, List<string>>();
        List<string> suggestions = new List<string>();
        string currentSection = null;
        bool isSuggestions = false;

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("Section"))
                {
                    currentSection = line.Substring(line.IndexOf(' ') + 1);
                    sections[currentSection] = new List<string>();
                    isSuggestions = false;
                }
                else if (line.StartsWith("Suggestions:"))
                {
                    isSuggestions = true;
                }
                else if (!string.IsNullOrEmpty(currentSection) && line.Contains(".") && !isSuggestions)
                {
                    var content = line.Split(new char[] { '.' }, 2)[1].Trim();
                    sections[currentSection].Add(content);
                }
                else if (isSuggestions && line.Contains("."))
                {
                    var suggestion = line.Split(new char[] { '.' }, 2)[1].Trim();
                    suggestions.Add(suggestion);
                }
            }
        }

        return new AvatarData
        {
            Persona = persona,
            Voice = voices[new System.Random().Next(voices.Count)],
            Sections = sections,
            Suggestions = suggestions
        };
    }

    private static void SaveAsJson(AvatarData data, string fileName)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(Path.Combine("Assets", "StreamingAssets", fileName), json);
    }
}
