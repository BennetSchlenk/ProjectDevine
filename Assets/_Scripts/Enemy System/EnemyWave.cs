using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    [SerializeField] private float waitBeforeStartingThisWave;
    [SerializeField] private Enemy enemy;
    [SerializeField] private int howManyInTheWave = 5;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private Sprite displaySprite;

    // getters
    public float WaitBeforeStartingThisWave => waitBeforeStartingThisWave;
    public Enemy Enemy => enemy;
    public int HowManyInTheWave => howManyInTheWave;
    public float SpawnInterval => spawnInterval;
    public Sprite DisplaySprite => displaySprite;

}
