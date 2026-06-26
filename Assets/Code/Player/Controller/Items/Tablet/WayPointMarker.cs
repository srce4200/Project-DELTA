using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WayPointMarker : MonoBehaviour
{ 
    public Image img;  
    public RawImage image;
    public TextMeshProUGUI meter; 
    public Vector3 offset;
    Camera mainCam;
    private void Start()
    {
        mainCam = Camera.main;
    }
    private void Update()
    { 
        float minX = img.GetPixelAdjustedRect().width / 2; 
        float maxX = Screen.width - minX;
         
        float minY = img.GetPixelAdjustedRect().height / 2; 
        float maxY = Screen.height - minY;
         
        Vector2 pos = mainCam.WorldToScreenPoint(transform.position + offset);

        if (Vector3.Dot((transform.position - mainCam.transform.position), mainCam.transform.forward) < 0)
        {
            if (pos.x < Screen.width / 2)
            {
                pos.x = maxX; 
            }
            else
            {
                pos.x = minX; 
            }
        } 

        // Limit the X and Y positions
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
         
        img.transform.position = pos; 
        meter.text = ((int)Vector3.Distance(mainCam.transform.position, transform.position)).ToString() + "m";
    }
    public void SetIcon(Texture icon)
    {
        image.texture = icon;
    }
}
