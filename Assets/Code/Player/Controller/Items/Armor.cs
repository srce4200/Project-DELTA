using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor : MonoBehaviour
{
    [Range(0f, 25f)]
    public float armorBuff;
    // Start is called before the first frame update
    void Start()
    {
        transform.root.GetComponent<playerHealth>().armorBuff += armorBuff;
    }
}
