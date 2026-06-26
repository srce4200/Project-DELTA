using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class PlayerManager : MonoBehaviour
{	
	[SerializeField] GameObject spawnMenu;
    [Header("Squad Managment")]
	[SerializeField] GameObject squadUIPrefab;
	[SerializeField] Transform squadList;
	[SerializeField] GameObject SquadChooseMenu;
	[Space]
	[SerializeField] Transform playerInSquadList;
	[SerializeField] GameObject InSquadMenu;
	[SerializeField] GameObject playerInSquadUIPrefab;

    [Header("Player class")]
	public playerClass currentPlayerClass;	
	public enum playerClass { rifleman, squadLeader, antiTank, medic, grenadier, machineGunner, marksman };
	public string routeToClasses;
	[SerializeField] TextMeshProUGUI roleText;

	[Header("Respawning")]
	[SerializeField] TextMeshProUGUI respawnTicketsText;
	[SerializeField] GameObject spawnUiPrefab;
	[SerializeField] Transform spawnpointsList;
    [SerializeField] TextMeshProUGUI currentSpawnText;
	[SerializeField] GameObject spawnUnavaibleNull;
    SpawnManager currentSpawn;

    PhotonView PV;

	GameObject spectator;
	GameObject controller;

	GameObject mapManager;
	PhotonTeamsManager photTeamsManager;
	PhotonTeam pt;

	Hashtable customProperty = new Hashtable();
	void Awake()
	{
		PV = GetComponent<PhotonView>();
		if (PV.IsMine)
		{
			CreateSpectator();
            mapManager = MapInfo.Instance.gameObject;
            photTeamsManager = mapManager.GetComponent<PhotonTeamsManager>();
			StartCoroutine(SpawnLateCheck());
			FillSquadMenu();
			ChangeClass(0);			
			respawnTicketsText.SetText((mapManager.GetComponent<MapInfo>().TeamTickets).ToString());
		}
		else
        {
			Destroy(spawnMenu);
			mapManager = MapInfo.Instance.gameObject;
		}
	}
    private void Update()
    {
		if (!PV.IsMine)
		    return;
        
			
        if (Input.GetKeyDown(KeyCode.Escape))
        {
			EnableSpectator(false);
        }
		if(spawnMenu.activeSelf == true)
        {
			RefreshSquadMenu();
        }
	}

	#region Player Class
	public void ChangeClass(int currentClass)
	{
		switch ((playerClass)currentClass)
		{
			case playerClass.rifleman:
				currentPlayerClass = playerClass.rifleman;
				break;
			case playerClass.squadLeader:
				currentPlayerClass = playerClass.squadLeader;
				break;
			case playerClass.antiTank:
				currentPlayerClass = playerClass.antiTank;
				break;
			case playerClass.medic:
				currentPlayerClass = playerClass.medic;
				break;
			case playerClass.grenadier:
				currentPlayerClass = playerClass.grenadier;
				break;
			case playerClass.machineGunner:
				currentPlayerClass = playerClass.machineGunner;
				break;
            case playerClass.marksman:
                currentPlayerClass = playerClass.marksman;
                break;
        }

		//network player current class
		customProperty["Class"] = (int)currentPlayerClass;
		PhotonNetwork.LocalPlayer.CustomProperties = customProperty;

		roleText.SetText("Role: " + currentPlayerClass.ToString());
	}
	#endregion

	#region SquadManagment

	void FillSquadMenu()
    {		
		for (int i = 1; i < photTeamsManager.GetAvailableTeams().Length + 1; i++)
		{
			PhotonTeam team;
			photTeamsManager.TryGetTeamByCode((byte)i, out team);
			
			SquadListItem squadPrefabItem = Instantiate(squadUIPrefab, squadList).GetComponent<SquadListItem>();
			squadPrefabItem.SquadName = team.Name;
			squadPrefabItem.SquadInt = i;
			squadPrefabItem.playerManager = this;
		} 
	}

	public void JoinSquad(int squadID)
    {
		PhotonTeamExtensions.JoinTeam(PhotonNetwork.LocalPlayer, (byte)squadID);
		SquadChooseMenu.SetActive(false);
		InSquadMenu.SetActive(true);

		RefreshSquadMenu();
    }
	void RefreshSquadMenu()
    {
		Player[] players;
		bool bl = PhotonTeamExtensions.TryGetTeamMates(PhotonNetwork.LocalPlayer, out players);
		foreach (Transform child in playerInSquadList)
		{
			Destroy(child.gameObject);
		}
        if (bl)
        {		
			for (int i = 0; i < players.Length; i++)
			{
				print(players[i]);

                
				//print((int)players[i].CustomProperties["Class"]);
			    Instantiate(playerInSquadUIPrefab, playerInSquadList).GetComponent<SquadPlayerListItem>().SetUp(players[i], 0);
		    }
        }

		Instantiate(playerInSquadUIPrefab, playerInSquadList).GetComponent<SquadPlayerListItem>().SetUp(PhotonNetwork.LocalPlayer, (int)currentPlayerClass);
	}

	public void LeaveSquad()
	{
		PhotonTeamExtensions.LeaveCurrentTeam(PhotonNetwork.LocalPlayer);
		SquadChooseMenu.SetActive(true);
		InSquadMenu.SetActive(false);
		ChangeClass(0);
	}

	#endregion

	#region Spectator
	void CreateSpectator()
	{
		if (SpawnManager.spawnManagers.Count > 0)
		{
			Transform spawnpoint = SpawnManager.spawnManagers[0].GetSpawnpoint();
			spectator = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", "PlayerSpectator"), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
		}
		else
		{
            spectator = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player", "PlayerSpectator"), new Vector3(0, 0, 0), transform.rotation, 0, new object[] { PV.ViewID });
        }

		EnableSpectator(false);
	}
	public void EnableSpectator(bool enableDisable)
    {
		if (controller != null)
			return;

        if (enableDisable)
			Cursor.lockState = CursorLockMode.Locked;
		else
			Cursor.lockState = CursorLockMode.Confined;

		spawnMenu.SetActive(!enableDisable);
		spectator.GetComponent<Spectator>().enabled = enableDisable;
    }
    #endregion

    #region Respawn
	IEnumerator SpawnLateCheck()
	{
		yield return new WaitForSeconds(0.5f);
		PopulateSpawnList();
	}
	void PopulateSpawnList()
	{
        foreach (Transform child in spawnpointsList)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < SpawnManager.spawnManagers.Count; i++)
        {
            Instantiate(spawnUiPrefab, spawnpointsList).GetComponent<SpawnListItem>().SetUp(SpawnManager.spawnManagers[i], i);
        }
		CheckCurrentSpawn();
    }
	void CheckCurrentSpawn()
	{
		if(currentSpawn != null)
		{
            spawnUnavaibleNull.SetActive(false);
        }
        else
		{
            spawnUnavaibleNull.SetActive(true);
        }
    }
	public void SetSpawnPoint(int spawnInt)
	{
        currentSpawn = SpawnManager.spawnManagers[spawnInt];
        currentSpawnText.SetText(currentSpawn.spawnName);
		CheckCurrentSpawn();		
    }
    public void SpawnPlayer()
    {
		if(currentSpawn == null)
		{
			PopulateSpawnList();
            return;
		}
		Transform spawnpoint = currentSpawn.GetSpawnpoint();
        if (mapManager.GetComponent<MapInfo>().TeamTickets > 0)
		{
			spawnMenu.SetActive(false);
			spectator.SetActive(false);

			controller = PhotonNetwork.Instantiate(Path.Combine(MapInfo.FolderFactionPath, currentPlayerClass.ToString()), spawnpoint.position, spawnpoint.rotation, 0, new object[] { PV.ViewID });
			controller.GetComponent<playerHealth>().managerScript = this;
        }
		
	}
	public void PlayerKilled()
    {
		if (!PV.IsMine)
			return;
		PhotonNetwork.Destroy(controller);
        spectator.SetActive(true);
        spawnMenu.SetActive(true);

        PV.RPC(nameof(SubstractTicket), RpcTarget.All);
		respawnTicketsText.SetText((mapManager.GetComponent<MapInfo>().TeamTickets).ToString());
	}

	[PunRPC]
	void SubstractTicket()
    {
		mapManager.GetComponent<MapInfo>().TeamTickets -= 1;
    }
	#endregion

}