using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ParticleSystem;

public class ObjectPlacer : MonoBehaviour
{
    public GameObject objectsArrtributes;
    public Transform parametersList;
    public GameObject parameterPrefab;
    [Space]
    public GameObject objectInteractions;
    public Transform addedObjectsParent;
    public Camera cam;


    [HideInInspector]public List<MissionObject> objects = new List<MissionObject>();
    [HideInInspector]public List<MissionObject> photonSpawnObjects = new List<MissionObject>();
    public static ObjectPlacer instance;
    [Space]
    [SerializeField] LayerMask placableLayer;

    GameObject currentObject;

    #region sync parameters
    GameObject syncing;
    int objectsyncId;
    #endregion

    int lastObjectId = 0;

    private void Awake()
    {
        instance = this;
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete) && currentObject != null)
        {
            DeleteObject(currentObject);
        }
    }

    #region Object Interacrions
    public void OpenAtributes(GameObject go)
    {
        currentObject = go;
        objectsArrtributes.SetActive(true);
        PopulateAtributes();
    }
    public void SelectObject(GameObject go)
    {
        if (currentObject != go)
            currentObject = go;
        else
            MoveObject();
    }
    public void TrySync(GameObject syncOb, int syncObId)
    {
        if (syncing != null)
        {
            ObjectModuleInfoHolder[] infos = syncOb.transform.parent.GetComponentsInChildren<ObjectModuleInfoHolder>(); 
            //first ob ID
            foreach (MissionObject m in objects)
            {
                if (m.id == syncObId) //2nd object find based on Id
                {
                    //get needed module for sync to line visual
                    foreach (ObjectModuleInfoHolder info in infos)
                    {
                        if (info.objectID == syncObId) //module with same id as 2nd object
                        {
                            m.syncedToId = objectsyncId; //giv 2nd object id of 1st one
                            info.GetComponent<CustomObjectSettings>().DisplaySyncTo(syncing.transform, info.transform);
                        }
                    }
                }
                else if(m.id == objectsyncId) //find 1st object based on id
                {
                    //get needed module for sync to line visual
                    foreach (ObjectModuleInfoHolder info in infos)
                    {
                        if (info.objectID == objectsyncId) //module with same id as 1st object
                        {
                            m.syncedToId = syncObId; //giv 1st object id of 2nd one
                        }
                    }
                }
            }
            //reset at end
            syncing = null;
            objectsyncId = -1;
            return;
        }
        else if(syncing != syncOb) //set 1st Ob
        {
            Debug.Log("Sync activated, Select 2nd object.");
            syncing = syncOb;
            objectsyncId = syncObId;
        }
        else //reset
        {
            Debug.Log("cancel sync");
            syncing = null;
            objectsyncId = -1;
        }
    }
    void PopulateAtributes()
    {
        foreach (Transform childObject in parametersList.transform) //reset
        {
            Destroy(childObject.gameObject);
        }

        CustomObjectSettings customSet = currentObject.GetComponent<CustomObjectSettings>();
        parameter[] atr = customSet.RetrieveSettings();
        for (int i = 0; i < atr.Length; i++)
        {
            ParametersItem para = Instantiate(parameterPrefab, parametersList).GetComponent<ParametersItem>();
            para.Setup(atr[i]);
        }
    }
    public void SaveParams()
    {
        MissionObject misO = currentObject.GetComponentInChildren<EditorObject>().missionObject;
        misO.objectsArtributes.Clear();

        foreach(Transform childObject in parametersList.transform)
        {
            print(childObject.GetComponent<ParametersItem>().gameObject.name);
            misO.objectsArtributes.Add(childObject.GetComponent<ParametersItem>().ReturnValue());
        }
        currentObject.GetComponent<CustomObjectSettings>().SetSettings(misO.objectsArtributes.ToArray(), misO.syncedToId);

        objectsArrtributes.SetActive(false);
        currentObject = null;
    }
    void MoveObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000, placableLayer))
        {
            Vector3 hitPos = new Vector3(Mathf.Round(hit.point.x * 10.0f) * 0.1f, Mathf.Round(hit.point.y * 10.0f) * 0.1f, Mathf.Round(hit.point.z * 10.0f) * 0.1f);
            currentObject.transform.position = hitPos;
            SavePos();
        }
    }
    void SavePos()
    {
        MissionObject misO = currentObject.GetComponentInChildren<EditorObject>().missionObject;
        misO.position = currentObject.transform.position;
    }
    #endregion

    //PLACE OBJECT
    public void PlaceObject(ScriptableEditorObject prefabObject)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000))
        {
            //-------spawn object ------------------//
            Vector3 hitPos = new Vector3(Mathf.Round(hit.point.x * 10.0f) * 0.1f, Mathf.Round(hit.point.y * 10.0f) * 0.1f, Mathf.Round(hit.point.z * 10.0f) *0.1f);
            GameObject ob = Instantiate((GameObject)Resources.Load(prefabObject.pathToObject), hitPos, Quaternion.identity, addedObjectsParent);

            //------add object to save list--------//
            MissionObject missionObject = new MissionObject();
            missionObject.pathToObject = prefabObject.pathToObject;
            missionObject.position = ob.transform.position;
            missionObject.rotation = ob.transform.rotation;
            missionObject.photonSpawn = ob.GetComponent<PhotonView>();
            //-------In case of params-------------//
            if (ob.GetComponent<CustomObjectSettings>())
            {
                foreach (parameter strg in ob.GetComponent<CustomObjectSettings>().RetrieveSettings())
                    missionObject.objectsArtributes.Add(strg);
            }            

            //split objects
            if (missionObject.photonSpawn)
                photonSpawnObjects.Add(missionObject);
            else if (!missionObject.photonSpawn)
                objects.Add(missionObject);

            //add editor only stuff
            EditorObject objectInteraction = Instantiate(objectInteractions,ob.transform.position,Quaternion.identity, ob.transform).GetComponent<EditorObject>();
            objectInteraction.textureImage.texture = prefabObject.objectIcon;
            objectInteraction.mainCam = cam;
            objectInteraction.missionObject = missionObject;

            //id in json and in editor
            missionObject.id = lastObjectId;
            ob.GetComponent<ObjectModuleInfoHolder>().objectID = lastObjectId;
            lastObjectId++;
        }
    }
    //PLACE OBJECT ON MISSION LOAD IN EDITOR
    public void PlaceObjectEditorLoad(MissionObject misObj)
    {
        //-------spawn object ------------------//
        GameObject ob = Instantiate((GameObject)Resources.Load(misObj.pathToObject), misObj.position, misObj.rotation, addedObjectsParent);

        //------add object to save list--------//
        MissionObject missionObject = new MissionObject();
        missionObject = misObj;

        //-------In case of params-------------//
        if (ob.GetComponent<CustomObjectSettings>())
        {
            CustomObjectSettings set = ob.GetComponent<CustomObjectSettings>();
            set.SetSettings((misObj.objectsArtributes).ToArray(), misObj.syncedToId);
        }

        //split objects
        if (missionObject.photonSpawn)
        {
            photonSpawnObjects.Add(missionObject);
            //Debug.Log(missionObject.pathToObject);
        }
        else
        {
            objects.Add(missionObject);
            //Debug.Log(missionObject.pathToObject);
        }

        //---------    add editor only stuff   ---------//
        EditorObject objectInteraction = Instantiate(objectInteractions, ob.transform.position, Quaternion.identity, ob.transform).GetComponent<EditorObject>();
        objectInteraction.textureImage.texture = ob.GetComponent<ObjectModuleInfoHolder>().editorObject.objectIcon;
        objectInteraction.mainCam = cam;
        objectInteraction.missionObject = misObj;

        //assign ID, now needed for modules sync to
        ob.GetComponent<ObjectModuleInfoHolder>().objectID = missionObject.id;

        lastObjectId = missionObject.id + 1;
    }
    
    public void DeleteObject(GameObject prefabObject)
    {
        objects.Remove(prefabObject.GetComponentInChildren<EditorObject>().missionObject);
        Destroy(prefabObject);
    }
    public void Save()
    {
        print("Saving file.");
        SaveLoadEditor.Instance.SaveAllObjects(objects, photonSpawnObjects);
    }
    public void ExitToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
