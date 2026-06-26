using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class spawnOnTrigger : MonoBehaviour
{
    [SerializeField] string tagToTrigger;
    [SerializeField] string pathToObject;
    [SerializeField] Transform spawnPosition;
    [SerializeField] int amaunt;
    [SerializeField] float delay;

    bool hasSpawned = false;
    private void Start()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == tagToTrigger && hasSpawned == false)
        {
            StartCoroutine(Spawn());
            hasSpawned = true;
        }
    }
    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(delay);
        for(int i = 0; i < amaunt; i++)
        {
            PhotonNetwork.Instantiate(pathToObject, spawnPosition.position, spawnPosition.rotation);
        }
        Destroy(gameObject);
    }
}
