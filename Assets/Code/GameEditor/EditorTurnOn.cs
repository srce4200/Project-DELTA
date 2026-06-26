using System.Collections;
using UnityEngine;

public class EditorTurnOn : MonoBehaviour
{
    bool editorOn = false;
    public GameObject gameManager;
    public GameObject mission;
    [Space] 
    public GameObject editor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        editorOn = SaveLoadEditor.Instance;
        print(Launcher.Instance.GameMode);
        if (!editorOn)
        {
            gameManager.SetActive(true);
            if(Launcher.Instance.GameMode == -1)
                mission.SetActive(true);

            editor.SetActive(false);
        }
        else
        {
            gameManager.SetActive(false);
            mission.SetActive(false);
            editor.SetActive(true);
        }
    }
}
