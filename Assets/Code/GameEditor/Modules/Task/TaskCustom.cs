using System;
using UnityEngine;

public class TaskCustom : CustomObjectSettings
{
    public override void SetSettings(parameter[] p, int syncTo)
    {
        try
        {
            GetComponent<AddTask>().task.taskName = p[0].defaultValues[p[0].selectedValue];
            GetComponent<AddTask>().task.taskDescription = p[1].defaultValues[p[1].selectedValue];
           //print(p[2].selectedValue);
            GetComponent<AddTask>().task.taskType = (p[2].selectedValue);
            GetComponent<AddTask>().task.taskID = p[3].defaultValues[p[3].selectedValue];

            GetComponent<TriggerBase>().syncToId = syncTo;

            SettingsSave(p);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        
    }
}
