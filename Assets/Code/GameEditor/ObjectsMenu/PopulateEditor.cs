using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopulateEditor : MonoBehaviour
{
    Transform objectList;
    public List<ScriptableEditorObject> objects;
    public GameObject objectListPrefab;
    public ObjectPlacer objectPlacer;
    private void Start()
    {
        objectList = transform;
        foreach (ScriptableEditorObject objectElement in objects)
        {
            GameObject go = Instantiate(objectListPrefab, transform);
            go.GetComponent<EditorUiElement>().Setup(objectElement.objectName, objectElement.objectIcon, objectPlacer, objectElement);
        }
    }
}
