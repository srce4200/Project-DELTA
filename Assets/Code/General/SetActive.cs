using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	public string spawnPoint;
	[SerializeField] GameObject graphics;

	void Awake()
	{
		graphics.SetActive(false);
	}
}
