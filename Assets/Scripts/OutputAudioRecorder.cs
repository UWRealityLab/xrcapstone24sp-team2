using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class OutputAudioRecorder : MonoBehaviour
{
    internal string FILENAME;
    private int outputRate;
    private int headerSize = 44;
    private string fileName;
    private bool recOutput = false;
    private FileStream fileStream;
    float[] tempDataSource;
    public Button StartBTN;

    void Awake()
    {
        outputRate = AudioSettings.outputSampleRate;
    }

    public string FileName
    {
        get { return fileName; }
    }

    public void StartRecording()
    {
        FILENAME = "record " + UnityEngine.Random.Range(1, 1000);
        fileName = Path.Combine(Application.persistentDataPath, FILENAME + ".wav");
        if (!recOutput)
        {
            StartWriting(fileName);
            recOutput = true;
            StartBTN.image.color = Color.red;
            Debug.Log("Start Recording");
        }
        else
        {
            Debug.Log("Recording is in progress already");
        }
    }

    public void StopRecording()
    {
        if (recOutput)
        {
            recOutput = false;
            WriteHeader();
            StartBTN.image.color = Color.white;
            Debug.Log("Stop Recording");
        }
    }

    private void StartWriting(string name)
    {
        fileStream = new FileStream(name, FileMode.Create);
        var emptyByte = new byte();
        for (int i = 0; i < headerSize; i++)
        {
            fileStream.WriteByte(emptyByte);
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (recOutput)
        {
            ConvertAndWrite(data);
        }
    }

    private void ConvertAndWrite(float[] dataSource)
    {
        var intData = new Int16[dataSource.Length];
        var bytesData = new Byte[dataSource.Length * 2];
        var rescaleFactor = 32767;
        for (var i = 0; i < dataSource.Length; i++)
        {
            intData[i] = (Int16)(dataSource[i] * rescaleFactor);
            var byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        fileStream.Write(bytesData, 0, bytesData.Length);
        tempDataSource = new float[dataSource.Length];
        tempDataSource = dataSource;
    }

    private void WriteHeader()
    {
        fileStream.Seek(0, SeekOrigin.Begin);
        var riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);
        var chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);
        var wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);
        var fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);
        var subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);
        UInt16 one = 1;
        var audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);
        var numChannels = BitConverter.GetBytes((UInt16)2);
        fileStream.Write(numChannels, 0, 2);
        var sampleRate = BitConverter.GetBytes(outputRate);
        fileStream.Write(sampleRate, 0, 4);
        var byteRate = BitConverter.GetBytes(outputRate * 4);
        fileStream.Write(byteRate, 0, 4);
        UInt16 four = 4;
        var blockAlign = BitConverter.GetBytes(four);
        fileStream.Write(blockAlign, 0, 2);
        UInt16 sixteen = 16;
        var bitsPerSample = BitConverter.GetBytes(sixteen);
        fileStream.Write(bitsPerSample, 0, 2);
        var dataString = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(dataString, 0, 4);
        var subChunk2 = BitConverter.GetBytes(fileStream.Length - headerSize);
        fileStream.Write(subChunk2, 0, 4);
        fileStream.Close();
    }

    public void SendWavToOpenAI(string filePath, Action<string> callback)
    {
        StartCoroutine(UploadWavFile(filePath, callback));
    }

    private IEnumerator UploadWavFile(string filePath, Action<string> callback)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, Path.GetFileName(filePath), "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.openai.com/v1/audio/transcriptions", form))
        {
            www.SetRequestHeader("Authorization", "Bearer YOUR_API_KEY");

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