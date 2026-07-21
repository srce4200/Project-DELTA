using System.Linq;
using UnityEngine;

public class SpawnCustom : CustomObjectSettings
{
    public override void SetSettings(parameter[] p, int syncTo)
    {
        GetComponent<SpawnManager>().spawnName = p[0].defaultValues[0]; //value 0 is spawn

        SettingsSave(p);
    }
}
