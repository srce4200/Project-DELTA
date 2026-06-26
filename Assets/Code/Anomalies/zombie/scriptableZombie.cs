using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ZombieObject", order = 1)]
public class scriptableZombie : ScriptableObject
{
    public float health;
    public float damage;
    public float speed = 5;
    public float attackSpeed = 1;
    public float attackDistance = 2;
    public float detectDistance = 100;
    [Space]
    public float fovAngle = 120f;

    public float patrolDistance = 10;

    [Header("Audio")]
    public List<AudioClip> attackSfx = new List<AudioClip>();
    public List<AudioClip> chaseSfx = new List<AudioClip>();
}
