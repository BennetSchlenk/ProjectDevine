using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWaves", menuName = "Project Divine/Enemies/EnemyWavesSO")]
public class EnemyWavesSO : ScriptableObject
{
    [Header("Config")]
    [SerializeField] private float initialWaitTimeBeforeStarting;

    [Header("Waves")]
    [SerializeField] private List<EnemyWave> waves;

    public float InitialWaitTime;
    public List<EnemyWave> Waves;
}
