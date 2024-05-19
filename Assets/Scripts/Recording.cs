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
    [SerializeField] UnityEngine.Camera recordingCamera;
    int i;
    Vector3 lastPositionLeft;
    Vector3 lastPositionRight;
    public float speedLeft = 0;
    public float speedRight = 0;

    public float TimeT;


    public float totalMovement;
    public List<float> movementData;
    // Start is called before the first frame update
    void Start()
    {
        lastPositionLeft = Vector3.zero;
        lastPositionRight = Vector3.zero;
        totalMovement = 0;
        i = 0;
        TimeT = 0;
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TimeT += Time.deltaTime;
        i++;
        speedLeft = (controllerLeft.transform.position - lastPositionLeft).magnitude;
        lastPositionLeft = controllerLeft.transform.position;
        speedRight = (controllerRight.transform.position - lastPositionRight).magnitude;
        lastPositionRight = controllerRight.transform.position;
        totalMovement += (speedLeft + speedRight);
        if (i == 20)
        {
            movementData.Add(totalMovement);
            graphObject.GetComponent<Graph>().ShowGraph(movementData, (int _i) => Mathf.RoundToInt(_i * 1f * TimeT / 15f) + "s", (float _f) => Mathf.RoundToInt(_f) + "m/s");
            totalMovement = 0;
            i = 0;
        }
    }
}
