using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditorObject : MonoBehaviour
{
    public Image img;
    public RawImage textureImage;
    [HideInInspector] public Camera mainCam;
    [HideInInspector] public MissionObject missionObject;

    #region mouse CliCk Detection

    float lastClickTime = 0f;
    float doubleClickThreshold = 0.3f; // Time in seconds
    bool isSingleClick = true;
    bool dragging;
    public void Release()
    {
        dragging = false;
    }
    public void OnPointerClick()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            RightClick();
        }
        else
        {
            LeftClick();
        }
    }
    void RightClick()
    {
        ObjectPlacer.instance.TrySync(transform.parent.gameObject, missionObject.id);
    }
    void LeftClick()
    {
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            // Double Click Detected
            isSingleClick = false;
            CancelInvoke(nameof(TriggerSingleClick)); // Cancel scheduled single-click event
            ObjectPlacer.instance.OpenAtributes(transform.parent.gameObject);
        }
        else
        {
            isSingleClick = true;
            Invoke(nameof(TriggerSingleClick), doubleClickThreshold);
        }

        lastClickTime = Time.time;
        dragging = true;
    }

    private void TriggerSingleClick()
    {
        if (isSingleClick)
        {
            ObjectPlacer.instance.SelectObject(transform.parent.gameObject);
        }
    }

    #endregion

    private void Update()
    {
        Update3DInteractPos();
        if (dragging)
        {
            ObjectPlacer.instance.SelectObject(transform.parent.gameObject);
        }
    }
    private void Update3DInteractPos()
    {
        float minX = img.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = img.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        Vector2 pos = mainCam.WorldToScreenPoint(transform.position);

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
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        img.transform.position = pos;
    }
}
