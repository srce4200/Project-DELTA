using UnityEngine;

public class TaskSetCustom : CustomObjectSettings
{
    public override void SetSettings(parameter[] p, int syncTo)
    {
        GetComponent<SetTask>().taskID = p[0].defaultValues[p[0].selectedValue];

        GetComponent<TriggerBase>().syncToId = syncTo;

        SettingsSave(p);
    }
}
