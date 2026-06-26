using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SquadListItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI squadNameText;
    public PlayerManager playerManager;
    public string SquadName = null;
    public int SquadInt;

    // Start is called before the first frame update
    void Start()
    {
        squadNameText.SetText(SquadName);
    }
    public void OnClick()
    {
        playerManager.JoinSquad(SquadInt);
    }
}
