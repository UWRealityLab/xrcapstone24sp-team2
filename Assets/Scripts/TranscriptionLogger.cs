using System.IO;
using TMPro;
using UnityEngine;

// Logs the full transcription output to Assets/SpeechLog.txt
public class TranscriptionLogger : MonoBehaviour
{
    private TextMeshProUGUI _textComponent;
    private string _previousText;
    private string _filePath;
    private StreamWriter _streamWriter;

    private void Start()
    {
        _textComponent = GetComponent<TextMeshProUGUI>();
        _previousText = string.Empty;
        // Assets/SpeechLog.txt
        _filePath = Path.Combine(Application.dataPath, "SpeechLog.txt");
        // opens the file in truncate mode to clear the content
        _streamWriter = new StreamWriter(_filePath, false);
        _streamWriter.Close();
        // reopens the file in append mode
        _streamWriter = new StreamWriter(_filePath, true);
    }

    // Log the transcription when the text input field changes
    private void Update()
    {
        string currentText = _textComponent.text;
        if (currentText != _previousText)
        {
            LogTranscription(currentText);
            _previousText = currentText;
        }
    }

    // Log the transcription immediately
    private void LogTranscription(string transcription)
    {
        _streamWriter.WriteLine(transcription);
        _streamWriter.Flush();
    }

    // Cleanup
    private void OnDestroy()
    {
        if (_streamWriter != null)
        {
            _streamWriter.Close();
        }
    }
}
