using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class playerHealth : MonoBehaviour
{
    [HideInInspector] public PlayerManager managerScript;
    [HideInInspector] public PhotonView pv;

    [SerializeField] Vector3 respawnPoint;
    public float health;

    [PunRPC]
    [HideInInspector]public float currentHealth;

    [HideInInspector] public float armorBuff;

    CameraShake shake;

    [Header("Ui Effects")]
    [SerializeField] GameObject hurt;
    [SerializeField] GameObject heavyHurt;
    [SerializeField] GameObject almostDead;
    [SerializeField] GameObject DownedUI;
    [SerializeField] Slider timerSlider; 
    [Space]
    [SerializeField] Material normalCam;
    [SerializeField] Material bloodCam;

    [SerializeField] SkinnedMeshRenderer bodyRenderer;
    bool isDowned;
    private void Awake()
    {
        respawnPoint = gameObject.transform.position;
    }
    // Start is called before the first frame update
    void Start()
    {
        timerSlider.maxValue = health;
        currentHealth = health;
        shake = GetComponentInChildren<CameraShake>();
        pv = GetComponent<PhotonView>();
    } 
    public void TakeDamageNoRPC(float damage)
    {
        pv.RPC("TakeDamage", RpcTarget.All, (double)(damage * (1.0-(armorBuff / 100))));
    }
    [PunRPC]
    public void TakeDamage(double damage)
    {
        if (!pv.IsMine)
            return;

        if (damage > 0) 
            shake.Shake(0.1f, 0.1f);

        pv.RPC("UpdateHealth", RpcTarget.All, damage);

        GetComponent<Animator>().SetBool("isDowned", isDowned);
        GetComponent<CharacterController>().enabled = !isDowned;
    }

    [PunRPC]
    void UpdateHealth(double newHealth)
    {
        if (newHealth < 0 && newHealth >= -100F)
        {
            if (isDowned)
                currentHealth -= (float)newHealth;
            else
                currentHealth = health;
        }
        else if (newHealth < -100 && isDowned)
        {
            currentHealth = health;
        }
        else
        {
            currentHealth -= (float)newHealth;
        }

        if (currentHealth <= 0)
        {
            GetDowned();
        }
        else //reset
        {
            isDowned = false;
            GetComponent<ChatControl>().EnableMouseInput(false);
            GetComponent<ChatControl>().LockControls(false, false); //in case revive
        }

        GetComponent<Animator>().SetBool("isDowned", isDowned);
        DownedUI.SetActive(isDowned);
        HandleUi();
    }
    void HandleUi()
    {
        if (currentHealth == health)
        {
            hurt.SetActive(false);
            heavyHurt.SetActive(false);
            almostDead.SetActive(false);
            ChangeMaterial(normalCam);
        }
        else if (currentHealth < health && currentHealth > 60)
        {
            hurt.SetActive(true);
            heavyHurt.SetActive(false);
            almostDead.SetActive(false);
            ChangeMaterial(bloodCam);
        }
        else if (currentHealth < 60 && currentHealth > 30)
        {
            hurt.SetActive(true);
            heavyHurt.SetActive(true);
            almostDead.SetActive(false);
            ChangeMaterial(bloodCam);
        }
        else if (currentHealth < 30)
        {
            hurt.SetActive(true);
            almostDead.SetActive(true);
            heavyHurt.SetActive(false);
            ChangeMaterial(bloodCam);
        }
    }
    void ChangeMaterial(Material material)
    {/*
        Material[] materials = bodyRenderer.materials;
        materials[0] = material;
        materials[1] = material;
        bodyRenderer.materials = materials;*/
    }

    void GetDowned()
    {
        timerSlider.maxValue = health;
        timerSlider.value = 100 - currentHealth * -1 / 10;

        GetComponent<ChatControl>().EnableMouseInput(true);
        GetComponent<ChatControl>().LockControls(true, true); 
        isDowned  = true;
        if (currentHealth < -1000 && pv.IsMine)
        {
            Die();
        }
    }
    public void Die() 
    {
        managerScript.PlayerKilled();
    }
}
