using TMPro;
using UnityEngine;

public class MissionListPrefab : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI missionName;
    [SerializeField] TextMeshProUGUI mapName;
    [SerializeField] TextMeshProUGUI missionGamemode;
    SaveLoadEditor missionLoadEditor;
    HostCustomMission hostCustomMission;
    public void Setup(string name, string map, string gamemode, SaveLoadEditor ed, HostCustomMission ho)
    {
        missionLoadEditor = ed;
        hostCustomMission = ho;
        missionName.text = name;
        mapName.text = map;
        missionGamemode.text = gamemode;
    }
    public void ReturnMission()
    {
        if (hostCustomMission != null)
            hostCustomMission.LoadMission(missionName.text, mapName.text);
        else
            missionLoadEditor.LoadMission(missionName.text, mapName.text);
    }
}
