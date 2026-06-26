using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#region custom classes
[Serializable]
public class Mission
{
    public string missionName;
    public string mapName;
    public string pathToFaction;
    public List<MissionObject> objects = new List<MissionObject>();
    public List<MissionObject> photonSpawn = new List<MissionObject>();
}
[Serializable]
public class MissionObject
{
    public int id;
    public string pathToObject;
    public Vector3 position;
    public Quaternion rotation;
    public bool photonSpawn = false;
    public List<parameter> objectsArtributes = new List<parameter>();
    public int syncedToId = -1;
}

#endregion

public class SaveLoadEditor : MonoBehaviour
{
    [HideInInspector] public static SaveLoadEditor Instance;

    [Header("List")]
    public GameObject saveLoadUiElement;
    public Transform listTransform;
    public GameObject objectInteractions;
    [Space]
    public TMP_InputField missionNameField;
    public TMP_Dropdown mapDropdown;
    //public MissionRunner missionRunner;

    string currentFilePath;
    public string[] mapNames = { "CursedExpanse", "TestingMap", "Madeln" };
    void Start()
    {
        InvokeRepeating(nameof(UpdateMissionList), 0, 1);
    }
    void Awake()
    {
        if (Instance != null && Instance != this)//happens when back on menu, detects duplicate, deletes old
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region UI menu List
    public void UpdateMissionList()
    {
        if (listTransform == null)
            return;

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
                string missionName = f.Name;
                f.Name.Replace("." + mapName, "");
                Instantiate(saveLoadUiElement, listTransform).GetComponent<MissionListPrefab>().Setup(f.Name, mapName, "/", this, null);
            }
        }
    }
    #endregion

    #region Save
    public void NewMission()
    {
        if (string.IsNullOrEmpty(missionNameField.text))
        {
            return;
        }

        Mission mission = new Mission();
        mission.mapName = mapDropdown.options[mapDropdown.value].text;
        SaveMission(mission);

        missionNameField.text = "";
    }
    public void SaveAllObjects(List<MissionObject> placableObjects, List<MissionObject> logicObjects)
    {
        Mission mission = new Mission();
        mission.objects = placableObjects;
        mission.photonSpawn  = logicObjects;
        mission.mapName = currentFilePath.Substring(currentFilePath.LastIndexOf('.') + 1);

        SaveMission(mission); //translate to JSON
    }

    void SaveMission(Mission missionData)
    {
        missionData.missionName = missionNameField.text;

        string json = JsonUtility.ToJson(missionData);
        SaveNewMission(missionData.missionName, missionData.mapName, json); //save path
    }
    void SaveNewMission(string missionName,string mapName, string json)
    {
        File.WriteAllText(Application.dataPath + "/" + missionName + "." + mapName, json);
    }

    #endregion

    #region Load Mission Objects and Data
    //loads data path info from UI
    public void LoadMission(string missionName, string mapName) //.MapName gets saved in mission name for some reason
    {
        string saveString = File.ReadAllText(Application.dataPath + "/" + missionName);
        currentFilePath = Application.dataPath + "/" + missionName;

        Mission savedMission = JsonUtility.FromJson<Mission>(saveString);

        missionNameField.text = savedMission.missionName;
        mapDropdown.GetComponentInChildren<TextMeshProUGUI>().text = mapName;
    }
    //load objects to scene
    void LoadAllObjecs()
    {
        string saveString = File.ReadAllText(currentFilePath);
        Mission savedMission = JsonUtility.FromJson<Mission>(saveString);
        StartCoroutine(LoadObjects(savedMission));
    }
    private IEnumerator LoadObjects(Mission savedMission)
    {
        while (ObjectPlacer.instance == null)
        {
            yield return null; // Wait for the next frame
        }

        foreach (MissionObject missionObject in savedMission.objects)
        {
            ObjectPlacer.instance.PlaceObjectEditorLoad(missionObject);
            //Instantiate(Resources.Load(missionObject.pathToObject), missionObject.position, missionObject.rotation);
        }
        foreach (MissionObject missionObject in savedMission.photonSpawn)
        {
            //spawn with photonnetworking
        }
    }
    #endregion

    #region Level loading
    public void OpenMission()
    {
        if (File.Exists(currentFilePath))
        {
            print("Starting Editor for: " + currentFilePath);
            StartCoroutine(LoadSceneAsync(currentFilePath.Substring(currentFilePath.LastIndexOf('.') + 1)));
        }
    }
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
            yield return null; // Wait for the next frame
        }
        Debug.Log("Scene loaded successfully!");
        LoadAllObjecs();
    }
    #endregion

    #region DeleteMission
    public void DeleteMission()
    {
        print("Deleted: " + currentFilePath);
        if(File.Exists(currentFilePath)) 
            File.Delete(currentFilePath);
    }
    #endregion
}


