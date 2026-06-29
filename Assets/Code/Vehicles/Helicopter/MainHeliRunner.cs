using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public class MainHeliRunner : MonoBehaviour
{
    public HeliRotor MainRotorController;
    public HeliRotor SubRotorController;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    Rigidbody mainRigidbody;
    AudioSource helicopterSound;

    [Space]
    public float pitchForce = 10f;
    public float rollForce = 10f; 
    public float yawForce = 30f;
    [Space]
    public float startUpPower;
    public float currentEnginePower;
    public float EnginePower
    {
        get
        {
            return currentEnginePower;
        }
        set
        {
            if (value < 20)
                helicopterSound.pitch = Mathf.Clamp(value / 10, 0, 1.2f);
            MainRotorController.RotarSpeed = value * 80;
            SubRotorController.RotarSpeed = value * 60;
            currentEnginePower = value;
        }
    }
    public float engineLift = 0.15f;

    bool autoHover;
    bool isGrounded;
    bool engineOn;
    float upDownInput;
    private Vector2 yawMovement = Vector2.zero;
    float rotateQE;

    void Start()
    {
        mainRigidbody = GetComponent<Rigidbody>();
        helicopterSound = GetComponent<AudioSource>();

        //mainRigidbody.ResetInertiaTensor();
       // mainRigidbody.inertiaTensor = new Vector3(1f, 1f, 1f);
    }
    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 2f, groundMask);
        MainHeliRunnerVoid();
    }

    #region Input
    public void HandleInput(Vector2 yawMove, float QE, float ascendDescend, bool autoHoverOn)
    {
        yawMovement = yawMove;
        rotateQE = QE;
        upDownInput = ascendDescend;
        autoHover = autoHoverOn;
    }
    #endregion outpout

    void MainHeliRunnerVoid()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 2f, groundMask);

        if(EnginePower <= 0)
        {
            helicopterSound.enabled = false;
        }
        else
        {
            helicopterSound.enabled =true;
        }

        if (currentEnginePower < startUpPower)
        {
            if(upDownInput > 0) 
            { 
                EnginePower += Time.deltaTime * engineLift;
            }
            else if (isGrounded && upDownInput < 0 && currentEnginePower > 0)
            {
                EnginePower -= Time.deltaTime * engineLift;
            }
        }
        else
        {
            if(isGrounded && upDownInput < 0)
            {
                EnginePower -= Time.deltaTime * engineLift;
                return;
            }

            AscendDescend();

            if (!isGrounded)
            {
                if (autoHover)
                    AutoHoverOn();
                else
                    HeliCopterRotate();
            } 
        }
    }

    void AutoHoverOn()
    {
        float rotateAngle = rotateQE * yawForce;

        Quaternion autoHRot = Quaternion.Euler(0f, rotateAngle, 0f);
        mainRigidbody.MoveRotation(mainRigidbody.rotation * autoHRot);
    }

    void AscendDescend()
    {
        if(upDownInput > 0)
        {
            mainRigidbody.AddRelativeForce(Vector3.up * upDownInput * 2);
        }
        else if(upDownInput < 0)
        {
            mainRigidbody.AddRelativeForce(Vector3.up * upDownInput * 2);
        }

        mainRigidbody.AddRelativeForce(Vector3.up * (mainRigidbody.mass * Mathf.Abs(Physics.gravity.y)));
    }

    void HeliCopterRotate()
    {
        float pitchAngle = -yawMovement.x * pitchForce;
        float rollAngle = -yawMovement.y * rollForce;
        float rotateAngle = rotateQE * yawForce;

        // Apply the rotation to the helicopter's rigidbody
        //Quaternion rotation = Quaternion.Euler(pitchAngle, rotateAngle, rollAngle);
        mainRigidbody.AddRelativeTorque(new Vector3(pitchAngle, rotateAngle, rollAngle), ForceMode.Force);
    } 
}
