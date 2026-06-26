using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatArea : MonoBehaviour
{
    public float warningTime = 10f;
    public TextMeshProUGUI warningText;

    playerHealth player;
    bool inCombatArea = true;
    float warningTimer;
    bool hasTakenDamage;
    private void Awake()
    {
        GetComponent<Renderer>().enabled = false;
    }

    private void Update()
    {
        if (!inCombatArea)
        {
            if (warningTimer <= 0f)
            {
                if (!hasTakenDamage) // check if player has already taken damage
                {
                    player.TakeDamage(1000); // deal damage to player
                    hasTakenDamage = true; // set flag to true
                }
            }
            else
            {
                warningTimer -= Time.deltaTime;
                warningText.enabled = true;
                warningText.text = "Return to combat area! You have " + Mathf.CeilToInt(warningTimer) + " seconds remaining.";
            }
        }
        else
        {
            warningText.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ermacore faction"))
        {
            if(other.GetComponent<PhotonView>().IsMine)
            {
                inCombatArea = true;
                warningText.enabled = false;
                player = null;
                hasTakenDamage = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ermacore faction"))
        {
            if (other.GetComponent<PhotonView>().IsMine)
            {
                inCombatArea = false;
                warningTimer = warningTime;
                player = other.GetComponent<playerHealth>();
            }
        }
    }
}
