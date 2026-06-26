using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
public class DropLogic : MonoBehaviour
{
    [SerializeField] GameObject parachut;
    [SerializeField] Transform groundCheck;
    [SerializeField] float DeleteBeforeGround = 5f;
    [SerializeField] float fallSpeed = 2f;
    [SerializeField] LayerMask groundLayer;
    Rigidbody rb;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isGrounded = Physics.CheckSphere(groundCheck.position, DeleteBeforeGround, groundLayer);
        rb.linearDamping = fallSpeed;
        if (isGrounded)
        {
            EndDropLogic();
        }
    }
    void EndDropLogic()
    {
        rb.linearDamping = 0.05f;
        Destroy(parachut);
        Destroy(this);
    }
}
