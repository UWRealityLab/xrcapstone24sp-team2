using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using OpenAI;

public class ChatGPTManager : MonoBehaviour
{
    public OnResponseEvent OnResponse;
    public UnityEvent OnResponsesReady;

    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string> { }

    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();

    /// <summary>
    /// Initiates an asynchronous interaction with the ChatGPT model based on the combined content of "Prompt.txt" and "SpeechLog.txt".
    /// </summary>
    /// <remarks>
    /// Reads the content from "Prompt.txt" and "SpeechLog.txt" located within the Assets/TextFiles directory,
    /// combines the content, logs it for debugging purposes, and checks for null or empty input.
    /// If valid, the combined text is sent as a request to the ChatGPT model. The model's response is then captured,
    /// logged, and the associated event is triggered with the response data.
    /// </remarks>
    /// <exception cref="Exception">Throws an exception if there is an error reading from the text files.</exception>
    public async void AskChatGPT()
    {
        // Read the text from Assets/TextFiles/Prompt.txt
        string promptFilePath = Path.Combine(Application.dataPath, "TextFiles", "Prompt.txt");
        string promptText = "";
        try
        {
            promptText = File.ReadAllText(promptFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading from the Prompt.txt file: " + e.Message);
            return;
        }

        // Read the text from Assets/TextFiles/SpeechLog.txt
        string speechLogFilePath = Path.Combine(Application.dataPath, "TextFiles", "SpeechLog.txt");
        string speechLogText = "";
        try
        {
            speechLogText = File.ReadAllText(speechLogFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading from the SpeechLog.txt file: " + e.Message);
            return;
        }

        // Combine the content of Prompt.txt and SpeechLog.txt
        string combinedText = promptText + "\n" + speechLogText;

        // Log the combined text content for debugging
        Debug.Log("Combined text from Prompt.txt and SpeechLog.txt: " + combinedText);

        // Check if the combined content is empty
        if (string.IsNullOrEmpty(combinedText))
        {
            Debug.LogError("The combined content is empty or null.");
            return;
        }

        // Ask ChatGPT
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = combinedText;
        newMessage.Role = "user";
        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        // request.Model = "gpt-3.5-turbo";
        request.Model = "gpt-4-turbo";

        // var response = await openAI.CreateChatCompletion(request);
        // if (response.Choices != null && response.Choices.Count > 0)
        // {
        //     var chatResponse = response.Choices[0].Message;
        //     messages.Add(chatResponse);

        //     // Log the response for debugging
        //     Debug.Log(chatResponse.Content);
        //     OnResponse.Invoke(chatResponse.Content);
        // }
        var response = await openAI.CreateChatCompletion(request);
        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            // Log the response for debugging
            Debug.Log(chatResponse.Content);
            OnResponse.Invoke(chatResponse.Content);

            // Separate professional and novice content
            string professionalContent = ExtractContent(chatResponse.Content, "Professional:");
            string noviceContent = ExtractContent(chatResponse.Content, "Novice:");

            // Save professional content to Response1.txt
            string response1FilePath = Path.Combine(Application.dataPath, "TextFiles", "Response1.txt");
            File.WriteAllText(response1FilePath, professionalContent);

            // Save novice content to Response2.txt
            string response2FilePath = Path.Combine(Application.dataPath, "TextFiles", "Response2.txt");
            File.WriteAllText(response2FilePath, noviceContent);
        }
        // Trigger files are updated
        OnResponsesReady.Invoke(); 
    }

    /// <summary>
    /// Extracts the content between the specified keyword and the next keyword or end of string.
    /// </summary>
    /// <param name="content">The input string to extract from.</param>
    /// <param name="keyword">The keyword to search for.</param>
    /// <returns>The extracted content.</returns>
    private string ExtractContent(string content, string keyword)
    {
        int startIndex = content.IndexOf(keyword);
        if (startIndex != -1)
        {
            startIndex += keyword.Length;
            int endIndex = content.IndexOf("Professional:", startIndex);
            if (endIndex == -1)
                endIndex = content.IndexOf("Novice:", startIndex);
            if (endIndex == -1)
                endIndex = content.Length;

            return content.Substring(startIndex, endIndex - startIndex).Trim();
        }

        return string.Empty;
    }

    void Start()
    {
        AskChatGPT();
    }
}