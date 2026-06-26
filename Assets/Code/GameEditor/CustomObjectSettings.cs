using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class parameter
{
    public string paramName;
    public string[] defaultValues;
    public int selectedValue = 0;
    public ParameterType parameterType;
}
public enum ParameterType { input, dropdown, checkbox}
public class CustomObjectSettings : MonoBehaviour
{
    [SerializeField] parameter[] parameterValues;

    [SerializeField] GameObject syncToLinePrefab;

    public virtual void SetSettings(parameter[] p, int syncTo)
    {
        for(int i = 0; i < parameterValues.Length; i++)
        {
            parameterValues[i] = p[i];
        }
    }
    public virtual void SettingsSave(parameter[] entry)
    {
        for (int i = 0; i < parameterValues.Length; i++)
        {
            parameterValues[i] = entry[i];
        }
    }
    public virtual parameter[] RetrieveSettings()
    {
        return parameterValues;
    }
    public virtual parameter[] RetrieveSettingsNames()
    {
        return parameterValues;
    }
    public virtual void DisplaySyncTo(Transform pos1, Transform pos2)
    {
        Instantiate(syncToLinePrefab).GetComponent<SyncLineScript>().CheckLine(pos1, pos2);
    }
}
