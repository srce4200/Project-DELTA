using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
public class BluTracker : MonoBehaviour
{
    [SerializeField] Camera trackerCam;
    [SerializeField] Vector3 offset;
    [Space]
    [SerializeField] Transform taskList;
    [SerializeField] GameObject taskPrefab;
    MapInfo mapInfo;

    Transform player;
    Animator animator;
    bool fold;
    // Start is called before the first frame update
    void Awake()
    {
        mapInfo = MapInfo.Instance;
        animator = GetComponent<Animator>();
        if(transform.root.GetComponent<PhotonView>().IsMine != true)
        {
            Destroy(trackerCam.gameObject);
            Destroy(this);
        }
        player = transform.root;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            animator.SetTrigger("fold");
            fold = !fold;
        }
        if (!fold)
        {
            RefreshTasks();
            trackerCam.transform.rotation = Quaternion.Euler(90,0,0);
        }
    }

    public void RefreshTasks()
    {
        foreach (Transform task in taskList.transform)
        {
            Destroy(task.gameObject);
        }

        foreach (Task task in mapInfo.tasks)
        {
            GameObject prefab = Instantiate(taskPrefab, taskList.transform);
            prefab.GetComponent<listTaskItem>().FixDescription(task.taskName, task.taskDescription, task.taskIcon, task.position);
        }
    }
}
