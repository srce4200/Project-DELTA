using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorUiElement : MonoBehaviour
{
    [HideInInspector] public ScriptableEditorObject objectProperties;
    [HideInInspector] public ObjectPlacer objectPlacer;
    [SerializeField] TextMeshProUGUI objectName;
    [SerializeField] RawImage objectIcon;
    public void Setup(string objectNam, Texture objectTexture, ObjectPlacer objPlac, ScriptableEditorObject objectPrefab)
    {
        objectName.text = objectNam;
        objectIcon.texture = objectTexture;
        objectPlacer = objPlac;
        objectProperties = objectPrefab;
    }
    public void ElementClicked()
    {
        objectPlacer.PlaceObject(objectProperties);
    }
}
