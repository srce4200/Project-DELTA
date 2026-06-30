using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	public string spawnName;
    public static List<SpawnManager> spawnManagers = new List<SpawnManager>();

	[SerializeField] Transform[] spawnpoints;

	void Awake()
	{
        spawnManagers.Add(this);
    }
    private void OnDestroy()//cleanup of old spawn
    {
        if (spawnManagers.Contains(this))
        {
            spawnManagers.Remove(this);
        }
    }
    public Transform GetSpawnpoint()
	{
		return spawnpoints[Random.Range(0, spawnpoints.Length)].transform;
	}
}
