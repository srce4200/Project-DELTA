using Photon.Pun;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    public static DoorManager Instance;
    public DoorInteractible[] doors;
    PhotonView _pv;
    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
        Instance = this;
    }
    [ContextMenu("Find and Sort All Doors")]
    private void FindAllDoorsInEditor()
    {
        doors = FindObjectsByType<DoorInteractible>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToArray(); 
        EditorUtility.SetDirty(this);

        // 4. Mark the active scene dirty so Ctrl+S actually writes to disk
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
    public void ChangeDoorState(DoorInteractible dr, bool isOpen)
    {
        int drIndx = Array.IndexOf(doors, dr);
        _pv.RPC(nameof(ChangeState_RPC), RpcTarget.AllBuffered, isOpen, drIndx);
    }
    [PunRPC]
    void ChangeState_RPC(bool isOpen, int drIndx)//0-close, 1-open
    {
        doors[drIndx].OpenClose(isOpen);
    }
}
