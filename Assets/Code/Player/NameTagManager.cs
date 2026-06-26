using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NameTagManager : MonoBehaviour
{
    [SerializeField] TextMeshPro nameTagText;
    // Start is called before the first frame update
    void Start()
    {
        if(GetComponent<PhotonView>().IsMine)
            nameTagText.gameObject.SetActive(false);

        nameTagText.SetText(GetComponent<PhotonView>().Owner.NickName);
    }
}
