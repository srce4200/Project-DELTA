using UnityEngine;

public class CustomTriggerSettings : CustomObjectSettings
{
    public override void SetSettings(parameter[] p, int syncedTo)
    {
        GetComponent<TriggerModule>().triggerSize = int.Parse(p[0].defaultValues[p[0].selectedValue]); //value 0 is spawn
        GetComponent<TriggerModule>().triggerer = p[1].defaultValues[p[1].selectedValue];
        GetComponent<TriggerModule>().specificTargetName = p[2].defaultValues[p[2].selectedValue];
        //print("Setting size to " + paramEters[0]);

        SettingsSave(p);

        GetComponent<TriggerModule>().UpdateTriggerSize();
    }
}
