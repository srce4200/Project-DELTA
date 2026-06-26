using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DeltaEye : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float boostSpeed;
    [SerializeField] float mouseSensitivity;
    PhotonView PV;


    [SerializeField] string generalEnemyPath;
    [SerializeField] Camera mainCam;
    [SerializeField] GameObject rotationalMenu;
    bool active;

    [SerializeField] Transform debugSphere;
    void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        RaycastHit hit;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

        if (Physics.Raycast(ray, out hit))
        {
                debugSphere.transform.position = hit.point;
        }


        if (Input.GetKey(KeyCode.Mouse1))
        {
            Cursor.lockState = CursorLockMode.Locked; 
            Look();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            
        }
    }

    #region ObjectSpawning

    public void SpawnObject(string path)
    {
        RaycastHit hit;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

        if (Physics.Raycast(ray, out hit))
        {
            PhotonNetwork.InstantiateRoomObject((generalEnemyPath + path).ToString(), hit.point, Quaternion.Euler(0, 0, 0));
        }
    }

    public void SpawnHorde_10(string path)
    {
        RaycastHit hit;
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        for(int i = 0; i < 10; i++)
        {
            if (Physics.Raycast(ray, out hit))
            {
                PhotonNetwork.InstantiateRoomObject((generalEnemyPath + path).ToString(), hit.point, Quaternion.Euler(0, 0, 0));
            }
        }
    }

    #endregion

    #region Movement
    void Movement()
    {
        //simple movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.position = transform.position + move * Time.deltaTime * boostSpeed;
        }
        else
        {
            transform.position = transform.position + move * Time.deltaTime * speed;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.position = transform.position + (transform.up * speed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            transform.position = transform.position + (-transform.up * speed * Time.deltaTime);
        }
    }
    void Look()
    {
        float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
        float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
    }
    #endregion
}
