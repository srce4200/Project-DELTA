using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsDisplay;
    [SerializeField] TextMeshProUGUI msDisplay;
    void Start()
    {
        InvokeRepeating(nameof(DisplayFps), 1, 1);
        InvokeRepeating(nameof(DisplayMs), 1, 1);
    }
    void DisplayFps()
    {
        float fps = (int)(1f / Time.unscaledDeltaTime);
        fpsDisplay.SetText(fps + "FPS");
    }
    void DisplayMs()
    {
        int ping = PhotonNetwork.GetPing();
        msDisplay.SetText(ping + " ms");
    }
}
