using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Position")]
    public float amount = 0.02f,
       maxAmount = 0.06f,
       smoothAmount = 6f;

    [Header("Rotation")]
    public float rotationAmount = 4f,
        maxRotationAmount = 5f,
        smoothRotation = 12f;

    [Space]
    public bool rotationX = true, rotationY = true, rotationZ = true;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private float InputX, InputY;
    PhotonView PV;
    private void Start()
    {
        PV = transform.root.GetComponent<PhotonView>(); 
        if (!PV.IsMine)
            Destroy(this);

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        CalculateSway();
        MoveSway();
        TiltSway();
    }
    float reduction;
    public void Aiming(bool isAiming)
    {
        if (isAiming)
            reduction = 3;
        else
            reduction = 1;
    }


    private void CalculateSway()
    {
        InputX = Input.GetAxis("Mouse X");
        InputY = Input.GetAxis("Mouse Y");
    }

    private void MoveSway()
    {
        float moveX = Mathf.Clamp(InputX * amount / reduction, -maxAmount, maxAmount / reduction);
        float moveY = Mathf.Clamp(InputY * amount / reduction, -maxAmount, maxAmount / reduction);

        Vector3 finalPosition = new Vector3(moveX, moveY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }

    private void TiltSway()
    {
        float tiltY = Mathf.Clamp(InputX * rotationAmount / reduction, -maxRotationAmount, maxRotationAmount / reduction);
        float tiltX = Mathf.Clamp(InputY * rotationAmount / reduction, -maxRotationAmount, maxRotationAmount / reduction);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(rotationX ? -tiltX : 0f, rotationY ? tiltY : 0f, rotationZ ? -tiltY : 0f));

        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation * initialRotation, smoothRotation * Time.deltaTime);
    }
}
