using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UI;
using UnityEngine.Windows;
using UnityEngine.Rendering.PostProcessing;
using System;

[Serializable]
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponStats", order = 1)]
public class WeaponStats : ScriptableObject
{
    public string weaponName;

    public enum FireMode { semi, auto, semi_auto, semi_auto_underbarrel };
    public FireMode fireMode = FireMode.semi;

    public float weaponFirerate;
    public string bulletName;

    public int ammoInMag;

    [Header("Recoil")]
    [Range(0.0f, 5f)] public float xRecoil;
    [Range(0f, 10f)] public float yRecoil;
    [Range(0.0f, 5f)] public float randomFactor;
    [Range(5f, 150f)] public float sidewayRecoil;
    public float layingReduceRecoil;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;

    

    [Header("Animations")]
    public RuntimeAnimatorController animator;
    public float reloadSpeed;

    [HideInInspector]
    public RuntimeAnimatorController animatorUnder;
    [HideInInspector]
    public float reloadSpeedUnder;
    [HideInInspector]
    public AudioClip fireSoundUnder;
    [HideInInspector]
    public int ammoInUnderbarrel;
}

#if UNITY_EDITOR
[CustomEditor(typeof(WeaponStats))]
public class RandomScript_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // for other non-HideInInspector fields

        WeaponStats script = (WeaponStats)target;

        // draw checkbox for the bool
        if (script.fireMode == WeaponStats.FireMode.semi_auto_underbarrel) // if bool is true, show other fields
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("GL", EditorStyles.boldLabel);

            script.animatorUnder = EditorGUILayout.ObjectField("Animator Under", script.animatorUnder, typeof(RuntimeAnimatorController), true) as RuntimeAnimatorController;
            script.reloadSpeedUnder = EditorGUILayout.FloatField("Reload Speed Under", script.reloadSpeedUnder);
            script.ammoInUnderbarrel = EditorGUILayout.IntField("Ammo In Underbarrel", script.ammoInUnderbarrel);
            script.fireSoundUnder = EditorGUILayout.ObjectField("Fire Sound Under", script.fireSoundUnder, typeof(AudioClip), true) as AudioClip;

            EditorUtility.SetDirty(target);
        }
    }
}
#endif
