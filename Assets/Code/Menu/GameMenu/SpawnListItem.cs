using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI spawnName;
    int spawn;
    PlayerManager pM;
    private void Awake()
    {
        pM = GetComponentInParent<PlayerManager>();
    }
    // Start is called before the first frame update
    public void SetUp(SpawnManager spMn, int spawnIndex)
    {
        spawnName.SetText(spMn.spawnName);
        spawn = spawnIndex;
        GetComponentInChildren<Button>().onClick.AddListener(OnClick);
    }
    void OnClick() 
    {
        pM.SetSpawnPoint(spawn);
    }
}
