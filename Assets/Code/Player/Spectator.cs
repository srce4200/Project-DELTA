using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Spectator : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float boostSpeed;
    [SerializeField] float mouseSensitivity;
    [SerializeField] Camera spectatorCam;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        if (PV != null && !PV.IsMine)
            Destroy(gameObject);
    }
    void Update()
    {
        Movement();
        if(Input.GetKey(KeyCode.Mouse1))
            Look();
    }

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
