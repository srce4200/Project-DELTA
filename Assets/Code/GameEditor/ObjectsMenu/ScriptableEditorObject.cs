using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/EditorObject", order = 1)]
public class ScriptableEditorObject : ScriptableObject
{
    public Texture objectIcon;
    public string objectName;
    public string pathToObject;
}
