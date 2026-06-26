using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using TMPro;

[System.Serializable]
public class ZombieList
{
    public string pathToZombie;
    public int zombieAmaunt;
}
[System.Serializable]
public class Wave
{
    public List<ZombieList> enemyPaths = new List<ZombieList>();

    public float timeBeforeWave;
}

public class Extraction : MonoBehaviour
{
    public GameUIManager gameUIManager;
    [Header("-----Waves-----")]
    int currentWave = 0;

    [SerializeField] string generalEnemyPath;
    public List<Wave> waveList = new List<Wave>();
    public List<Transform> zombieSpawnPoints = new List<Transform>();

    [Header("Extract")]
    [SerializeField] AnimatorOverrideController heliExtractAnimation;
    [SerializeField] Transform heliSpawnIn;
    [SerializeField] int WaveToSpawnHeli;

    [Header("Completed")]
    //[SerializeField] Trigger missionCompletedTrigger;

    PhotonView _pv;

    // Start is called before the first frame update
    private void OnEnable()
    {
        _pv = gameObject.GetComponent<PhotonView>();
        StartCoroutine(gameUIManager.DisplayWarning("Extraction initializing...", 10f));

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(SpawnWave());
    }

    #region Waves

    IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(waveList[currentWave].timeBeforeWave);

        if (currentWave == WaveToSpawnHeli)
            CallExtract();        

        float spawnMultiplier = PhotonNetwork.CurrentRoom.PlayerCount * 0.75f;

        for (int j = 0; j < waveList[currentWave].enemyPaths.Count; j++)
        {
            for(int i = 0; i < waveList[currentWave].enemyPaths[j].zombieAmaunt * spawnMultiplier; i++)
            {
                Transform spawnPoint = zombieSpawnPoints[Random.Range(0, zombieSpawnPoints.Count - 1)];
                PhotonNetwork.InstantiateRoomObject((generalEnemyPath + waveList[currentWave].enemyPaths[j].pathToZombie).ToString(), spawnPoint.position, spawnPoint.rotation);
            }
        }

        _pv.RPC(nameof(GlobalWaveUpdate), RpcTarget.AllBufferedViaServer);

        yield return new WaitForSeconds(5);

        if (currentWave < waveList.Count)
        {
            StartCoroutine(SpawnWave());
        }        
    }

    [PunRPC]
    void GlobalWaveUpdate()
    {
        if (currentWave == WaveToSpawnHeli)
            StartCoroutine(gameUIManager.DisplayInfo(("Extraction en route"), 20f));

        currentWave += 1;
        StartCoroutine(gameUIManager.DisplayWarning(("Wave " + (currentWave) + " spawned"), 5f));
    }
    #endregion

    #region Extraction
    void CallExtract()
    {
        int obj = PhotonNetwork.InstantiateRoomObject(MapInfo.FolderFactionPath + "HeliExtract", heliSpawnIn.position, heliSpawnIn.rotation).GetComponent<PhotonView>().ViewID;
        _pv.RPC("HeliExtractAnim", RpcTarget.All, obj);

        InvokeRepeating(nameof(CheckWhenCompleted), 30f, 1f);
    }

    [PunRPC]
    void HeliExtractAnim(int obj)
    {
        Animator my = PhotonNetwork.GetPhotonView(obj).GetComponent<Animator>();
        my.runtimeAnimatorController = heliExtractAnimation;
    }

    void CheckWhenCompleted()
    {
        /*if(missionCompletedTrigger.triggerCompleted)
        {
            gameUIManager.MissionCompleted();
            _pv.RPC("LoadScene", RpcTarget.All);
        }*/
    }
    #endregion
    [PunRPC]
    void LoadScene()
    {
        StartCoroutine(LoadSeyquence());    
    }
    IEnumerator LoadSeyquence()
    {
        yield return new WaitForSeconds(3f);
        Destroy(RoomManager.Instance.gameObject);
        PhotonNetwork.Disconnect();
        yield return new WaitForSeconds(0.2f);

        PhotonNetwork.LoadLevel(0);
    }
}
