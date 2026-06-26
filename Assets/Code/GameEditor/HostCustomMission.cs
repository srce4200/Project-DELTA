using System.IO;
using TMPro;
using UnityEngine;

public class HostCustomMission : MonoBehaviour
{
    [Header("List")]
    public GameObject saveLoadUiElement;
    public Transform listTransform;
    [Space]
    string currentFilePath;
    public string[] mapNames = { "CursedExpanse", "TestingMap" };
    
    public TMP_InputField missionNameField;
    public TMP_Dropdown mapDropdown;
    private void Start()
    {
        InvokeRepeating(nameof(UpdateMissionList), 0, 1);
    }

    #region UI menu List
    public void UpdateMissionList()
    {
        foreach (Transform child in listTransform)
        {
            Destroy(child.gameObject);
        }

        //get all created missions
        foreach(string mapName in mapNames)
        {
            DirectoryInfo info = new DirectoryInfo(Application.dataPath);
            FileInfo[] files = info.GetFiles("*." + mapName);
            foreach (FileInfo f in files)
            {
                f.Name.Replace(".*", "");
                Instantiate(saveLoadUiElement, listTransform).GetComponent<MissionListPrefab>().Setup(f.Name, mapName, "/", null, this);
            }
        }
    }
    #endregion

    public void LoadMission(string missionName, string mapName) //.MapName gets saved in mission name for some reason
    {
        string saveString = File.ReadAllText(Application.dataPath + "/" + missionName);
        currentFilePath = Application.dataPath + "/" + missionName;

        Mission savedMission = JsonUtility.FromJson<Mission>(saveString);

        missionNameField.text = savedMission.missionName;
        mapDropdown.GetComponentInChildren<TextMeshProUGUI>().text = mapName;

        Launcher.Instance.SetMissionData(saveString);
    }
}
