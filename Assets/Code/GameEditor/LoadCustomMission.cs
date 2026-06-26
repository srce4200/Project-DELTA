using System.Collections;
using TMPro;
using UnityEngine;

public class LoadCustomMission : MonoBehaviour
{
    [SerializeField] Transform addedObjectsParent;
    private void Awake()
    {
        DecrypString(Launcher.Instance.missionData);
    }
    public void DecrypString(string missionData)
    {
        Mission savedMission = JsonUtility.FromJson<Mission>(missionData);
        LoadObjects(savedMission);
    }
    private void LoadObjects(Mission savedMission)
    {
        print("Loading customs scenario objects...");
        foreach (MissionObject missionObject in savedMission.objects)
        {
            GameObject ob = Instantiate((GameObject)Resources.Load(missionObject.pathToObject), missionObject.position, missionObject.rotation, addedObjectsParent);
            if(ob.GetComponent<CustomObjectSettings>())
            {
                ob.GetComponent<CustomObjectSettings>().SetSettings(missionObject.objectsArtributes.ToArray(), missionObject.syncedToId);
            }
            ob.GetComponent<ObjectModuleInfoHolder>().objectID = missionObject.id;
            //Instantiate(Resources.Load(missionObject.pathToObject), missionObject.position, missionObject.rotation);
        }
        foreach (MissionObject missionObject in savedMission.photonSpawn)
        {
            //spawn with photonnetworking
        }
    }
}
