using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DemonObject", order = 1)]
public class ScriptableDemon : ScriptableObject
{
    public float health;
    public float damage;
    public float speed = 5;
    public float attackSpeed = 1;
    public float attackDistance = 2;
    public float detectDistance = 100;
    [Space]

    public float fleeDistance = 50;
    public float patrolDistance = 50;
    public float stalkDistance = 60;
    public float fovAngle = 120f;

    [Header("Audio")]
    public List<AudioClip> attackSfx = new List<AudioClip>();
    public List<AudioClip> chaseSfx = new List<AudioClip>();
}
