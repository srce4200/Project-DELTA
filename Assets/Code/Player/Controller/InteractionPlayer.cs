using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
public class InteractionPlayer : MonoBehaviour
{
    Camera cam;
    public float range = 2f;
    PhotonView PV;
    public GameObject interactUi;

    public Image img;
    Vector3 offset;
    [SerializeField]Camera mainCam;
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        if (!PV.IsMine)
            return;
        cam = gameObject.GetComponentInChildren<Camera>();
    }
    void Update()
    {
        if (!PV.IsMine)
            return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            Interactable script = hit.transform.GetComponent<Interactable>();
            if(script != null)
            {
                interactUi.SetActive(true);
                Update3DInteractPos(script.transform);
                //interactionText.SetText(script.InteractionDescription());
                if (Input.GetKeyDown(KeyCode.F))
                    script.Interact(gameObject);
            }
            else
            {
                interactUi.SetActive(false);
            }
        }
        else
        {
            interactUi.SetActive(false);
        }
    }
    private void Update3DInteractPos(Transform posTargetObject)
    {
        float minX = img.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = img.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        Vector2 pos = mainCam.WorldToScreenPoint(posTargetObject.position + offset);

        if (Vector3.Dot((posTargetObject.position - mainCam.transform.position), mainCam.transform.forward) < 0)
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

