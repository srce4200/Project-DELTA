using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

public class BuildController : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject debugSphere;
    string currentObjectPath;
    bool modeBuildOn = false;

    GameObject preview;
    [Space]
    [SerializeField] Material previeMat;
    [SerializeField] Material previeMatBad;
    // Start is called before the first frame update
    void Start()
    {
        this.enabled = false;
    }
    private void OnEnable()
    {
        debugSphere.SetActive(true);
    }
    private void OnDisable()
    {
        debugSphere.SetActive(false);
        GetComponent<ChatControl>().LockMouseMovement(true);
    }
    // update is called once per frame
    public void SetObject(string path, bool buildModeOn)
    {
        currentObjectPath = path;
        modeBuildOn = buildModeOn;
        if(path != null)
        {
            preview = (GameObject)Instantiate(Resources.Load(path));
            preview.GetComponent<Collider>().enabled = false;
        }
        else
        {
            preview = null;
        }
    }
    void Update()
    {
        RaycastHit hit;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

        if (Physics.Raycast(ray, out hit, 10f))
        {
            if (preview == null)
                preview = debugSphere;

            preview.GetComponentInChildren<Renderer>().material = previeMat;
            preview.transform.position = hit.point;
            preview.transform.rotation = gameObject.transform.rotation;

            if (modeBuildOn)
            {
                //placement
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    print("place");
                    GetComponent<PhotonView>().RPC("PlaceObject", RpcTarget.All, hit.point, currentObjectPath);
                    this.enabled = false;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    this.enabled = false;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && hit.collider.tag == "emplacement")
                {
                    print("delete");
                    hit.collider.gameObject.GetComponent<DeleteObject>().Delete();
                    this.enabled = false;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    this.enabled = false;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                this.enabled = false;
            }
            preview.GetComponentInChildren<Renderer>().material = previeMatBad;
        }
    }
    [PunRPC]
    void PlaceObject(Vector3 spawnPos, string objectPath)
    {
        Destroy(preview);
        if (PhotonNetwork.IsMasterClient)
        {
            Quaternion spawnRotation = gameObject.transform.rotation;
            PhotonNetwork.Instantiate(objectPath, spawnPos, spawnRotation);
        }
    }
}
