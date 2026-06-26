using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SupportScriptable", order = 1)]

public class SupportScriptable : ScriptableObject
{
    public string supportName = "Unknown";
    public int supportPrice = 500;
    public Texture supportIcon;
    public GameObject supportsPrefab;
}
