using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class PlayerMovement : MonoBehaviour
{
    [Header("Stamina")]
    [SerializeField] Slider staminaSlider;
    [SerializeField] float staminaRegen, staminaDecreaseTimer, staminaRegenTime;
    float curStaminaRegenTime, stamina = 70f;
    public float currentStamina;

    [Header("Movement")]
    [SerializeField] float walkSpeed, sprintSpeed;
    float currentSpeed;

    [Header("Jump")]
    [SerializeField] float jumpHeight;
    [SerializeField] float gravity;
    public LayerMask ground;
    Vector3 velocity;

    [Header("Stances")]
    [SerializeField] GameObject standingUI;
    [SerializeField] GameObject crouchingUI;
    bool crouched;
    bool laying;

    [Header("Player Look")]
    public float mouseSensitivity;
    public float smoothTime;
    [SerializeField] Transform spine001;
    [SerializeField] Transform camHolder;
    public Transform recoilPivot;

    float minFov = 50;
    float curFov = 75;

    Quaternion XtargetRotation;
    float xRotation, freeLookX, freeLookY, leanRotation;

    float lastAltPressTime = 0f;

    [Header("Audio")]
    [SerializeField] AudioClip walkSound;
    [SerializeField] AudioClip sprintSound;
    AudioSource _audioSource;

    [Header("Other")]
    [SerializeField] CharacterController controller;
    [SerializeField] GameObject player;
    [SerializeField] Animator playerAnimatorCont;
    [SerializeField] WeaponSway weaponSway;
    [SerializeField] Camera mainCam;
    public bool isAiming;
    PhotonView PV;

    void Awake()
    {
        currentStamina = stamina;
        currentSpeed = walkSpeed;
        PV = GetComponent<PhotonView>();
        _audioSource = GetComponent<AudioSource>();
        staminaSlider.maxValue = stamina;
    }
    private void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(mainCam.gameObject);
        }

        //initialize options script
        if (PlayerPrefs.HasKey("Sensitivity"))
            mouseSensitivity = PlayerPrefs.GetFloat("Sensitivity");
    }

    void Update()
    {
        if (!PV.IsMine) 
            return;
        weaponSway.Aiming(isAiming);

        Look();
        Movement();
        Jump();
        FallDamage();
        CrouchLay();
    }

    #region -WALKING, RUNNING, SPRINTING-
    void Movement()
    {
        RegenStamina();

        //simple movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = player.transform.right * x + player.transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && !isAiming && !crouched && !laying && currentStamina > 0) //sprint
            {
                PlaySound(sprintSound);
                Sprinting();
            }
            else //walk
            {
                PlaySound(walkSound);
                Walking();
            }
        }
        else
        {
            _audioSource.enabled = false;
            playerAnimatorCont.SetBool("isSprinting", false);
            playerAnimatorCont.SetBool("isWalking", false);
        }
        
    }
    void Walking()
    {
        playerAnimatorCont.SetBool("isSprinting", false);
        playerAnimatorCont.SetBool("isWalking", true);
        if (crouched)
            currentSpeed = walkSpeed / 2f;
        else if (laying)
            currentSpeed = walkSpeed / 4f;
        else
            currentSpeed = walkSpeed;        
    }
    void Sprinting()
    {
        playerAnimatorCont.SetBool("isWalking", true);
        playerAnimatorCont.SetBool("isSprinting", true);
        currentSpeed = sprintSpeed;
    }
    void PlaySound(AudioClip sound)
    {
        _audioSource.clip = sound;
        if (!_audioSource.isPlaying)
        {           
            _audioSource.enabled = true;
            _audioSource.Play(); 
        }       
    }
    void RegenStamina()
    {
        staminaSlider.value = currentStamina;
        staminaSlider.gameObject.SetActive(currentStamina != stamina);

        if (sprintSpeed == currentSpeed)
        {
            currentStamina = Mathf.Clamp(currentStamina - (staminaDecreaseTimer * Time.deltaTime), 0.0f, stamina);
            curStaminaRegenTime = 0.0f;
        }
        else if (currentStamina < stamina)
        {
            if (curStaminaRegenTime >= staminaRegenTime)
            {
                currentStamina = Mathf.Clamp(currentStamina + (staminaRegen * Time.deltaTime), 0.0f, stamina);
            }
            else
            {
                curStaminaRegenTime += Time.deltaTime;
            }
        } 
    }

    #endregion

    #region CrouchAndLayDown

    void CrouchLay()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            crouched = !crouched;
            laying = false;
        }
        /*if(Input.GetKeyDown(KeyCode.Z) && Application.systemLanguage == SystemLanguage.English || Input.GetKeyDown(KeyCode.Y) && !(Application.systemLanguage == SystemLanguage.English ))
        {
            crouched = false;
            laying = !laying;
        }*/
            
        playerAnimatorCont.SetBool("isCrouched", crouched);
        playerAnimatorCont.SetBool("isLaying", laying);
        if(!crouched && !laying)
        {
            standingUI.SetActive(true);
            crouchingUI.SetActive(true);
        }
        else
        {
            standingUI.SetActive(false);
            crouchingUI.SetActive(crouched);
        }
    }

    #endregion

    #region -JUMPING-Fall Damage-
    void Jump()
    {        
        //Gravity
        controller.Move(velocity * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -1.3f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && !laying && currentStamina >= 6)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -1 * gravity);
            currentStamina -= 5f;
        }
    }
    void FallDamage()
    {
        if(!controller.isGrounded && velocity.y < -18f)
        {
            GetComponent<playerHealth>().TakeDamage(-velocity.y);
        }
    }
    #endregion

    #region -PLAYER LOOK-
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 10 * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 10 * Time.deltaTime;

        if (laying)//player laying
        {
            xRotation = Mathf.Clamp(xRotation, -100, -60);
        }
        else
        {
            xRotation = Mathf.Clamp(xRotation, -130, -30);
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        //FREE LOOK
        if (!Input.GetKey(KeyCode.LeftAlt))
        {
            //whole player body
            xRotation -= mouseY;
            float yRot = Mathf.Lerp(spine001.localRotation.y, mouseX, smoothTime);
            transform.Rotate(Vector3.up * yRot);

            //spine
            XtargetRotation = Quaternion.Euler(xRotation, 0, leanRotation);

            //freelook----
            camHolder.localEulerAngles = new Vector3(0, 0, 0);
            freeLookY = freeLookX = 0;
        }
        else //freelook
        {
            freeLookX -= mouseY;
            float targetxRot = Mathf.Clamp(freeLookX, -30, 50);

            freeLookY += mouseX;
            float targetyRot = Mathf.Clamp(freeLookY, -120, 120);

            camHolder.localEulerAngles = new Vector3(targetxRot, targetyRot, 0);
        }
        //---------
        Lean();
        ZoomIn();

        spine001.localRotation = Quaternion.Lerp(spine001.localRotation, XtargetRotation, smoothTime * Time.deltaTime);        
    }
    void ZoomIn()
    {
        if (Input.GetKey(KeyCode.Mouse4)) {
            GetComponent<ProceduralAim>().ZoomInNOut(true, 1.3f);
        }
        else if (Input.GetKey(KeyCode.Mouse3))
        {
            GetComponent<ProceduralAim>().ZoomInNOut(true, 0.8f);
        }
        else 
        {
            GetComponent<ProceduralAim>().ZoomInNOut(false, 1);
        }
    }
    void Lean()
    {
        if (Input.GetKey(KeyCode.Q) && currentSpeed != sprintSpeed && !laying)
        {
            leanRotation = 30;
        }
        else if (Input.GetKey(KeyCode.E) && currentSpeed != sprintSpeed && !laying)
        {
            leanRotation = -30;
        }
        else
        {
            leanRotation = 0;
        }
    }
    #endregion
}
