using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliInput : MonoBehaviour
{
    public MainHeliRunner heliScript;
    public bool InputActive;
    bool autoHoverOn;
    // Update is called once per frame
    void Update()
    {
        if(InputActive)
            SendInput();
    }
    void SendInput()
    {
        Vector2 rotation;
        rotation.x = Input.GetAxis("Horizontal");
        rotation.y = Input.GetAxis("Vertical");
        float rotate = Input.GetAxis("Yaw");
        float enginePower = Input.GetAxis("EnginePower");

        if (Input.GetKeyDown(KeyCode.X))
            autoHoverOn = !autoHoverOn;
        heliScript.HandleInput(rotation, rotate, enginePower, autoHoverOn);
    }
}
