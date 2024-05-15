using UnityEngine;
using TMPro;

public class PaceDisplayController : MonoBehaviour
{
    public TranscriptionLogger transcriptionLogger;
    public TextMeshProUGUI paceInfoText;

    void Update()
    {
        if (transcriptionLogger == null || paceInfoText == null)
            return;

        paceInfoText.text = $"Overall Average WPM: {transcriptionLogger.GetOverallAverage():F2}\n";
        paceInfoText.text += "Section Averages:\n";

        foreach (float avg in transcriptionLogger.GetSectionAverages())
        {
            paceInfoText.text += $"{avg:F2} WPM\n";
        }
    }
}