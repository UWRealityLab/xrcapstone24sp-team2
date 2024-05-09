using System.IO;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System;


using System.Threading.Tasks;

using UnityEngine.Events;
using Newtonsoft.Json;

public class TranscriptionLogger : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _textComponent;
    private string _previousText;
    private string _filePath;

    private List<string> transcriptionList;

    private void Start()
    {
        _previousText = string.Empty;
        transcriptionList = new List<string>();
    }

    private void Update()
    {
        string currentText = _textComponent.text;
        if (currentText != _previousText)
        {
            transcriptionList.Add(currentText);
            _previousText = currentText;
        }
    }

    public void ResetTranscript()
    {
        _previousText = string.Empty;
        _textComponent.text = string.Empty;
        transcriptionList.Clear();
    }
    public List<string> GetTranscriptionList()
    {
        return transcriptionList;
    }
}

