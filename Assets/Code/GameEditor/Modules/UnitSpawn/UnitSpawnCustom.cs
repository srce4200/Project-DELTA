using UnityEngine;

public class UnitSpawnCustom : CustomObjectSettings
{
    public override void SetSettings(parameter[] p, int syncTo)
    {
        Debug.Log("Setting settings to " + syncTo);
        GetComponent<UnitSpawn>().unitAmaunt = int.Parse(p[0].defaultValues[0]); //value 0 is spawn
        GetComponent<UnitSpawn>().pathToUnit = p[1].defaultValues[p[1].selectedValue];

        GetComponent<TriggerBase>().syncToId = syncTo;

        SettingsSave(p);
    }
}
