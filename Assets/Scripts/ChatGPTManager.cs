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
    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string> { }

    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();

    /// <summary>
    /// Initiates an asynchronous interaction with the ChatGPT model based on the content of a specified text file.
    /// </summary>
    /// <remarks>
    /// Reads a user's input from "SpeechLog.txt" located within the Assets directory, logs the content for debugging purposes,
    /// and checks for null or empty input. If valid, the text is sent as a request to the ChatGPT model. The model's response
    /// is then captured and logged, and the associated event is triggered with the response data.
    /// </remarks>
    /// <exception cref="Exception">Throws an exception if there is an error reading from the text file.</exception>
    public async void AskChatGPT()
    {
        // Read the text from Assets/SpeechLog.txt
        string filePath = Path.Combine(Application.dataPath, "SpeechLog.txt");
        string newText;
        try
        {
            newText = File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error reading from the SpeechLog.txt file: " + e.Message);
            return;
        }

        // Log the text content for debugging
        Debug.Log("Text read from SpeechLog.txt: " + newText);

        // Check if the file content is empty
        if (string.IsNullOrEmpty(newText))
        {
            Debug.LogError("The SpeechLog.txt file is empty or text is null.");
            return;
        }

        // Ask ChatGPT
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        messages.Add(newMessage);
        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        // request.Model = "gpt-3.5-turbo";
        request.Model = "gpt-4-turbo";

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            // Log the response for debugging
            Debug.Log(chatResponse.Content);

            OnResponse.Invoke(chatResponse.Content);
        }
    }

    void Start()
    {
        AskChatGPT();
    }
}
