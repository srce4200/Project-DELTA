using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Seat", order = 3)]
public class ScriptableSeat : ScriptableObject
{
    public string weaponName;

    public enum FireMode { semi, auto, semi_auto };
    public FireMode fireMode = FireMode.semi;

    public float weaponFirerate;
    public string bulletName;

    public int ammoInMag; 

    [Header("Audio")]
    public AudioClip fireSound; 
} 
