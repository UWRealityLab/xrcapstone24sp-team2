using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.IO;

public class OpenAITTS : MonoBehaviour
{
    [SerializeField] private string openAIURL = "https://api.openai.com/v1/audio/speech";
    [HideInInspector] public string apiKey; 

    public IEnumerator GetTTS(string jsonData, Action<AudioClip> callback)
    {
        Debug.Log("Sending JSON: " + jsonData); // Log JSON string debugging

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
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                callback?.Invoke(clip);
            }
            else
            {
                Debug.LogError($"TTS Request Error: {www.error}");
                callback?.Invoke(null);
            }
        }
    }

    private IEnumerator Start()
    {
        yield return LoadApiKey();
        // After this point, the apiKey should be loaded and available for use.
    }

    private IEnumerator LoadApiKey()
    {
        string path = Path.Combine(Application.dataPath, "APIKey.txt");
        if (File.Exists(path))
        {
            apiKey = File.ReadAllText(path).Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("API Key is empty in the file.");
            }
        }
        else
        {
            Debug.LogError("API Key file not found at: " + path);
        }
        yield break;
    }
}
