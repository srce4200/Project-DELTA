using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquadPlayerListItem : MonoBehaviour
{
	[SerializeField] TMP_Text text;
	[SerializeField] RawImage classIcon;
	int playerClassInt;
	Player player;

	[Header("Icons")]
	[SerializeField] Texture riflemanIcon;
	[SerializeField] Texture squadLeadIcon;
	[SerializeField] Texture ATicon;
	[SerializeField] Texture medicIcon;
	[SerializeField] Texture GLicon;
	[SerializeField] Texture MGicon;
    [SerializeField] Texture MRKicon;

    public void SetUp(Player _player, int playerClass)
	{
		player = _player;
		text.text = _player.NickName;
		playerClassInt = playerClass;
		ClassIcon(playerClass);
	}
	void ClassIcon(int i)
    {
		if (playerClassInt == i)
			classIcon.texture = riflemanIcon;
		else if (playerClassInt == i)
			classIcon.texture = squadLeadIcon;
		else if (playerClassInt == i)
			classIcon.texture = ATicon;
		else if (playerClassInt == i)
			classIcon.texture = medicIcon;
		else if (playerClassInt == i)
			classIcon.texture = GLicon;
		else if (playerClassInt == i)
			classIcon.texture = MGicon;
        else if (playerClassInt == i)
            classIcon.texture = MRKicon;
    }
}
