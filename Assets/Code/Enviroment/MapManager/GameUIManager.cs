using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class Notification
{
    public int type = 0;
    public string description;
    public float time = 5f;
    public Texture imgIcon;
}
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance;
    public Transform notificationsList;
    [SerializeField] GameObject warningTextPrefab;
    [SerializeField] GameObject infoTextPrefab;
    List<Notification> notifications = new List<Notification>();
    int curNumOfNotifications = 0;
    
    [Space][SerializeField] GameObject taskNotifiy;
    public Transform notifyTasksList;
    List<Notification> taskNotifications = new List<Notification>();
    int curNumOfTaskNotifications = 0;

    [Space]
    [SerializeField] GameObject missionCompletedUI;
    [SerializeField] GameObject missionFailedUI;
    private void Start()
    {
        Instance = this;
    }
    public void QueueNotification(int type, string description)
    {
        Notification notify = new Notification();
        notify.type = type;
        notify.description = description;
        notifications.Add(notify);
    }
    #region TaskUI
    public void QueueNotificationTask(int type, string description)
    {
        Notification notify = new Notification();
        notify.type = type;
        notify.description = description;
        taskNotifications.Add(notify);
    }
    #endregion
    private void Update()
    {
        if (notifications.Count > 0 && curNumOfNotifications < 3) 
        {
            int notificationsNum = notifications.Count - 1;
            if (notifications[notificationsNum].type == 0)
            {
                StartCoroutine(DisplayInfo(notifications[notificationsNum].description, notifications[notificationsNum].time));
            }
            else if (notifications[notificationsNum].type == 1)
            {
                StartCoroutine(DisplayWarning(notifications[notificationsNum].description, notifications[notificationsNum].time));
            }
            notifications.Remove(notifications[notificationsNum]);
            curNumOfNotifications++;
        }


        if (taskNotifications.Count > 0 && curNumOfTaskNotifications < 3)
        {
            int notificationsNum = taskNotifications.Count - 1;

            StartCoroutine(DisplayTask(taskNotifications[notificationsNum].description, taskNotifications[notificationsNum].time));

            taskNotifications.Remove(taskNotifications[notificationsNum]);
            curNumOfTaskNotifications++;
        }
    }
    public IEnumerator DisplayWarning(string description, float time)
    {
        GameObject warningText = Instantiate(warningTextPrefab, notificationsList);
        warningText.GetComponent<Animator>().SetBool("Show", true);

        warningText.GetComponentInChildren<TextMeshProUGUI>().SetText(description);

        yield return new WaitForSeconds(time);

        warningText.GetComponent<Animator>().SetBool("Show", false);
        yield return new WaitForSeconds(1);
        curNumOfNotifications--;
        Destroy(warningText);
    }

    public IEnumerator DisplayInfo(string description, float time)
    {
        GameObject infoText = Instantiate(infoTextPrefab, notificationsList);
        infoText.GetComponent<Animator>().SetBool("Show", true);

        infoText.GetComponentInChildren<TextMeshProUGUI>().SetText(description);

        yield return new WaitForSeconds(time);

        infoText.GetComponent<Animator>().SetBool("Show", false);
        yield return new WaitForSeconds(1);
        curNumOfNotifications--;
        Destroy(infoText);
    }

    public IEnumerator DisplayTask(string description, float time)
    {
        GameObject warningText = Instantiate(taskNotifiy, notifyTasksList);
        warningText.GetComponent<Animator>().SetBool("Show", true);

        warningText.GetComponentInChildren<TextMeshProUGUI>().SetText(description);

        yield return new WaitForSeconds(time);

        warningText.GetComponent<Animator>().SetBool("Show", false);
        yield return new WaitForSeconds(1);
        curNumOfTaskNotifications--;
        Destroy(warningText);
    }

    public void MissionCompleted()
    {
        missionCompletedUI.SetActive(true);
    }
    public void MissionFailed()
    {
        missionFailedUI.SetActive(true);
    }
}
