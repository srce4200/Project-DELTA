using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Survival : MonoBehaviour
{
    [SerializeField] List<GameObject> combatZones = new List<GameObject>();
    [Header("-----Waves-----")]
    [SerializeField] GameUIManager gameUIManager;
    int currentWave = 0;

    [SerializeField] string generalEnemyPath;
    public List<Wave> waveList = new List<Wave>();
    List<Transform> zombieSpawnPoints = new List<Transform>();
    List<GameObject> zombies = new List<GameObject>();
     
    [SerializeField] PhotonView _pv;
    bool zoneSelected = false;
    int currentZone;
    // Start is called before the first frame update
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!zoneSelected)
            {
                currentZone = Random.Range(0, combatZones.Count);
                _pv.RPC(nameof(SelectRandomZone), RpcTarget.AllBufferedViaServer, currentZone);
            }
            else
            {
                zombieSpawnPoints = GetComponentInChildren<ZombieSpawnList>().spawnpoints;
                StartCoroutine(SpawnWave());
            }
        }
    }

    [PunRPC]
    void SelectRandomZone(int i)
    {
        combatZones[i].SetActive(true);
        zoneSelected = true;
        if(PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnWave());
            zombieSpawnPoints = GetComponentInChildren<ZombieSpawnList>().spawnpoints;
        }
    }

    #region Waves

    IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(waveList[currentWave].timeBeforeWave);

        float spawnMultiplier = PhotonNetwork.CurrentRoom.PlayerCount * 0.75f;
        for (int j = 0; j < waveList[currentWave].enemyPaths.Count; j++)
        {
            for (int i = 0; i < waveList[currentWave].enemyPaths[j].zombieAmaunt * spawnMultiplier; i++)
            {
                Transform spawnPoint = zombieSpawnPoints[Random.Range(0, zombieSpawnPoints.Count - 1)];
                GameObject go = PhotonNetwork.InstantiateRoomObject((generalEnemyPath + waveList[currentWave].enemyPaths[j].pathToZombie).ToString(), spawnPoint.position, spawnPoint.rotation);
                zombies.Add(go);
            }
        }

        gameUIManager.DisplayWarning("Wave " + (currentWave + 1) + " in proggress", 5f);
        _pv.RPC(nameof(GlobalWaveUpdate), RpcTarget.AllBufferedViaServer);

        while (zombies.Count > 0)
        {
            for (int i = zombies.Count - 1; i >= 0; i--)
            {
                if (zombies[i] == null)
                    zombies.RemoveAt(i);
            }
            yield return new WaitForSeconds(1f);
        }

        gameUIManager.DisplayWarning("Next Wave in " + waveList[currentWave].timeBeforeWave + "s", 5f);
        yield return new WaitForSeconds(5);

        StartCoroutine(SpawnWave());
    }

    [PunRPC]
    void GlobalWaveUpdate()
    {
        currentWave += 1; 
    }
    #endregion
}