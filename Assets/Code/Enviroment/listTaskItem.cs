using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class listTaskItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Toggle toggle;
    [SerializeField] RawImage icon;
    
    public Transform objectivePos;
    [SerializeField] GameObject obj3dMarkerPrefab;
    GameObject objMarker;
    public void FixDescription(string mainText, string sideText, Sprite tskIcon, Transform objPos)
    {
        nameText.text = mainText;
        descriptionText.text = sideText;
        objectivePos = objPos;
        icon.texture = tskIcon.texture;
    }
    public void Spawn3DMarker(bool active)
    {
        if (active)
        {
            objMarker = Instantiate(obj3dMarkerPrefab, objectivePos);
            objMarker.GetComponent<WayPointMarker>().SetIcon(icon.texture);
        }
        else
            Destroy3DMarker();
    }
    private void OnDestroy()
    {
        Destroy3DMarker();
    }
    public void Destroy3DMarker()
    {
        if(objMarker != null)
            Destroy(objMarker);
    } 
}
