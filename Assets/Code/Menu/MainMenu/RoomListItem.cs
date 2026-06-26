using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomListItem : MonoBehaviour
{
	[SerializeField] TMP_Text nameText;
	[SerializeField] TMP_Text playerText;
	[SerializeField] TMP_Text gamemodeText;
    [SerializeField] TMP_Text mapText;
	[SerializeField] TMP_Text factionText;

    public int GameMode;
	public RoomInfo info;

	public void SetUp(RoomInfo _info, int GameModeInfo, string mapName, string factionName)
	{
		//room name
		info = _info;
		nameText.text = _info.Name;

		//map name
		mapText.SetText(mapName);

		//gamemode
		if(GameModeInfo == 0) { gamemodeText.SetText("Hives"); }
		else if (GameModeInfo == 1) { gamemodeText.SetText("Survival"); }
		else if (GameModeInfo == 2) { gamemodeText.SetText("Extraction"); }
		else if (GameModeInfo == 3) { gamemodeText.SetText("Delta Eye"); }
		GameMode = GameModeInfo;

        //faction name
        factionText.SetText(factionName);

        //player
        int current = System.Convert.ToInt32(_info.PlayerCount);
		int max = System.Convert.ToInt32(_info.MaxPlayers);
		playerText.text = current + "/" + max;	
	}

	public void OnClick()
	{
		Launcher.Instance.JoinRoom(info, GameMode);
	}
}
