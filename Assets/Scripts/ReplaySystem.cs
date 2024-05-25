using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReplaySystem : MonoBehaviour
{
    Camera thisCam;

    public float delayBetweenFrames = 0.05f;
    private List<Texture2D> frames = new List<Texture2D>();
    private bool isRecording = false;
    private bool isPlaying = false;
    public Renderer quadRender;
    public RawImage theUI;

    // Start is called before the first frame update
    void Start()
    {
        thisCam = GetComponent<Camera>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        RecordFrame();
    }

    public void StartRecording()
    {
        frames.Clear();
        isRecording = true;
    }

    public void StopRecording()
    {
        isRecording = false;
        Microphone.End(null);
    }

    void RecordFrame()
    {
        if (isRecording)
        {
            Texture2D frame = CaptureFrame();
            frames.Add(frame);
        }
    }

    Texture2D CaptureFrame()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = thisCam.targetTexture;

        Texture2D frame = new Texture2D(thisCam.targetTexture.width, thisCam.targetTexture.height);
        frame.ReadPixels(new Rect(0, 0, thisCam.targetTexture.width, thisCam.targetTexture.height), 0, 0);
        frame.Apply();

        RenderTexture.active = currentRT;

        return frame;
    }

    void DisplayFrame(Texture2D frame)
    {
        quadRender.material.mainTexture = frame;
    }

    public void StartPlayback()
    {
        if (!isPlaying && frames.Count > 0)
        {
            StartCoroutine(Playback());
        }
    }

    public void Toggle()
    {
        if (!isRecording)
        {
            StartRecording();
        } else
        {
            StopRecording();
        }
    }

    IEnumerator Playback()
    {
        isPlaying = true;

        for (int i = 0; i < frames.Count; i++)
        {
            DisplayFrame(frames[i]);
            yield return new WaitForSeconds(delayBetweenFrames);
        }

        isPlaying = false;
    }
}
