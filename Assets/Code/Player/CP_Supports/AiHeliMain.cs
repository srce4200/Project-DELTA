using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiHeliMain : MonoBehaviour
{
    [SerializeField] float rotorSpeed;
    [SerializeField] GameObject mainRotor;
    [SerializeField] GameObject backRotor;
    //[SerializeField] Trigger triggerToLiftOff;
    [SerializeField] ParticleSystem dustEffect;
    [SerializeField] LayerMask groundMask;

    bool runOnce;

    // Update is called once per frame
    void Update()
    {
        SpinRotors();
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 5f, groundMask))
        {
            dustEffect.Play();
        }
        else
        {
            dustEffect.Stop();
        }

        /*if (triggerToLiftOff.triggerCompleted && !runOnce && PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(NextAnim());
            runOnce = true;
        }*/
    }
    IEnumerator NextAnim()
    {
        yield return new WaitForSeconds(15f);
        GetComponent<Animator>().SetBool("Extracted", true);
    } 
     
    void SpinRotors()
    {
        mainRotor.transform.Rotate(Vector3.forward * rotorSpeed * 100 * Time.deltaTime);
        backRotor.transform.Rotate(Vector3.left * rotorSpeed * 100 * Time.deltaTime);
    }  
}
