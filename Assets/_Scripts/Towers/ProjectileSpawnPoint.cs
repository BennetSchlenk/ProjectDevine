using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawnPoint : MonoBehaviour
{
    [Tooltip("Priority of this component. The tower root will sort all its ProjectileSpawnPoint based on this number (less means more priority).")]
    [SerializeField] private int priority = 0;
    public int Priority => priority;
}
