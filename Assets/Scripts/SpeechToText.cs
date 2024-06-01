using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class SpeechToText : MonoBehaviour
{
    private const string openAIApiUrl = "https://api.openai.com/v1/audio/transcriptions";
    private const string openAIModel = "whisper-1";
    string apiKey = OpenAIConfig.ApiKey;

    // Public method to start the transcription process
    public void TranscribeAudio(string filePath, Action<string> callback)
    {
        StartCoroutine(UploadAudio(filePath, callback));
    }

    private IEnumerator UploadAudio(string filePath, Action<string> callback)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, "audio.wav", "audio/wav");
        form.AddField("model", openAIModel);
        form.AddField("response_format", "text");

        using (UnityWebRequest www = UnityWebRequest.Post(openAIApiUrl, form))
        {
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to upload audio file: " + www.error);
            }
            else
            {
                var responseJson = www.downloadHandler.text;
                var transcriptionResponse = JsonUtility.FromJson<TranscriptionResponse>(responseJson);
                callback(transcriptionResponse.text);
            }
        }
    }

    [Serializable]
    private class TranscriptionResponse
    {
        public string text;
    }
}