using UnityEngine;
using TMPro;

// debugging only
public class PaceDisplayController : MonoBehaviour
{
    public TranscriptionLogger transcriptionLogger;
    [SerializeField] GameObject graphObject;
    [SerializeField] Recording time;

    public void CreateGraph()
    {
        graphObject.SetActive(true);
        graphObject.GetComponent<Graph>().ShowGraph(transcriptionLogger.GetSectionAverages(), (int _i) => Mathf.RoundToInt(_i * 1f * time.GetTime() / 15f) + "s", (float _f) => Mathf.RoundToInt(_f) + "WPM");
    }

    public void HideGraph()
    {
        graphObject.SetActive(false);
    }
}