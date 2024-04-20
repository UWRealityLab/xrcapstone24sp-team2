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
