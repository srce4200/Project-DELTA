using System.Linq;
using UnityEngine;

public class SpawnCustom : CustomObjectSettings
{
    public override void SetSettings(parameter[] p, int syncTo)
    {
        print("2. SetSettings running! Giving name: " + p[0].defaultValues[0]); // Runs too late!
    
    print(p);
        print(p[0]);
            print(" with value of " + p[0].defaultValues[0]);
        GetComponent<SpawnManager>().spawnName = p[0].defaultValues[0]; //value 0 is spawn

        SettingsSave(p);
    }
}
