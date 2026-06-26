using UnityEngine;

public class NotifyModule : CustomObjectSettings
{
    public override void SetSettings(parameter[] p, int syncedTo)
    {
        GetComponent<CallNotify>().type = p[0].selectedValue;
        GetComponent<CallNotify>().msg = p[1].defaultValues[p[1].selectedValue];

        GetComponent<TriggerBase>().syncToId = syncedTo;

        SettingsSave(p);
    }
}
