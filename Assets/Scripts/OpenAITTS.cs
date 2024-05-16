using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAITTS : MonoBehaviour
{
    [SerializeField] private string openAIURL = "https://api.openai.com/v1/audio/speech";
    string apiKey = OpenAIConfig.ApiKey;

    private void Awake()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("Failed to load API key for OpenAI TTS. Please ensure that the API key is correctly set in the OpenAIConfig.cs file.");
        }
    }

    public IEnumerator GetTTS(string jsonData, Action<AudioClip> callback)
    {
        Debug.Log($"[TTS Request] Sending JSON: {jsonData}");

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("[TTS Request] API Key is not loaded, aborting TTS request.");
            yield break;
        }

        using (UnityWebRequest www = new UnityWebRequest(openAIURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerAudioClip(www.url, AudioType.MPEG);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[TTS Request] Success");
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                callback?.Invoke(clip);
            }
            else
            {
                Debug.LogError($"[TTS Request] Error: {www.error}");
                callback?.Invoke(null);
            }
        }
    }
}