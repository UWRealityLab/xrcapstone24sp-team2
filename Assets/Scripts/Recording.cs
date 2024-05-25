using GLTFast.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Recording : MonoBehaviour
{
    [SerializeField] GameObject controllerLeft;
    [SerializeField] GameObject controllerRight;
    [SerializeField] GameObject graphObject;
    private Vector3 lastPositionLeft;
    private Vector3 lastPositionRight;
    private float speedLeft = 0;
    private float speedRight = 0;
    private Boolean recording = false;

    private float TimeT;


    public float totalMovement;
    public List<float> movementData;
    // Start is called before the first frame update
    void Start()
    {
        lastPositionLeft = controllerLeft.transform.position;
        lastPositionRight = controllerRight.transform.position;
        totalMovement = 0;
        TimeT = 0;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (recording)
        {
            TimeT += Time.deltaTime;
            speedLeft = (controllerLeft.transform.position - lastPositionLeft).magnitude;
            lastPositionLeft = controllerLeft.transform.position;
            speedRight = (controllerRight.transform.position - lastPositionRight).magnitude;
            lastPositionRight = controllerRight.transform.position;
            totalMovement += (speedLeft + speedRight);
        }
    }

    IEnumerator WaitAndUpload()
    {
        while (recording)
        {
            movementData.Add(totalMovement);
            totalMovement = 0;
            yield return new WaitForSeconds(2);
        }
    }

    public void CreateGraph()
    {
        graphObject.SetActive(true);
        graphObject.GetComponent<Graph>().ShowGraph(movementData, (int _i) => Mathf.RoundToInt(_i * 1f * TimeT / 15f) + "s", (float _f) => Mathf.RoundToInt(_f) + "m/s");
    }

    public void ToggleRecording()
    {
        recording = !recording;
        if (recording)
        {
            TimeT = 0;
            StartCoroutine(WaitAndUpload());
        }
    }

    public void ClearData()
    {
        movementData.Clear();
    }

    public void DisableGraph()
    {
        graphObject.SetActive(false);
        movementData.Clear();
    }
}
