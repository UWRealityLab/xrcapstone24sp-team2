using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        // Lock cursor.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        // Release cursor.
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
