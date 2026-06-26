using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField][HideInInspector] protected InventoryManager inventoryManager;
    [SerializeField][HideInInspector] protected PlayerMovement playerMove;
    [SerializeField][HideInInspector] protected Camera mainCam;
    [SerializeField][HideInInspector] protected PhotonView PV;

    [SerializeField][HideInInspector] protected bool isSprinting;

    [Header("Animations")]
    [SerializeField][HideInInspector] protected Animator handAnimations;
    [SerializeField][HideInInspector] protected AudioSource soundSource;
    [SerializeField][HideInInspector] protected bool isReloading;
    public virtual void Start()
    {
        playerMove = GetComponentInParent<PlayerMovement>();
        mainCam = playerMove.transform.GetComponentInChildren<Camera>();
        soundSource = GetComponent<AudioSource>();
        PV = GetComponent<PhotonView>();
    }
    public virtual void Setup()
    {

    }
    public virtual void OnEnable()
    {
        inventoryManager = transform.root.GetComponent<InventoryManager>();
        handAnimations.SetBool("isReloading", false);
        handAnimations.SetBool("LowReady", false);
    }
    public virtual void OnDisable()
    {
        handAnimations = inventoryManager.handAnimations;
    }
    public virtual void Animations_movement()
    {
        //animations
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A) && isReloading == false)
        {
            handAnimations.SetBool("isWalking", true);
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            if (playerMove.currentStamina > 0.2f)
            {
                handAnimations.SetBool("isSprinting", isSprinting);
            }
            else
            {
                handAnimations.SetBool("isSprinting", false);
            }
        }
        else
            handAnimations.SetBool("isWalking", false);
    }
}
