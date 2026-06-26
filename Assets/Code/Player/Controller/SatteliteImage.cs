using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatteliteImage : MonoBehaviour
{
    [SerializeField] Vector3 rotation;
    [SerializeField] Vector3 positionCam;
    [SerializeField] float MaxFov;
    [SerializeField] float MinFov;

    Camera cam;
    private void Start()
    {
        cam = GetComponent<Camera>();
    }
    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(rotation);

        if(Input.GetKey(KeyCode.Q) && cam.orthographicSize > MinFov)
        {
            cam.orthographicSize -= 20 * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E) && cam.orthographicSize < MaxFov)
        {
            cam.orthographicSize += 20 * Time.deltaTime;
        }
        Movement();
    }

    void Movement()
    {
        //simple movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.up * z;

        transform.position = new Vector3(transform.position.x, positionCam.y, transform.position.z) + move * Time.deltaTime * 10;
    }
}
