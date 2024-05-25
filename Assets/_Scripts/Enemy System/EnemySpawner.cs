using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("For Testing")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private int maxSpawnCount = 20;

    [SerializeField, Space] private Transform parentObjectForEnemies;


    public static EnemySpawner Instance;

    
    #region Unity Callbacks
        
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    #endregion

    private IEnumerator SpawnLoop()
    {
        for (int i = 0; i < maxSpawnCount; i++)
        {            
            yield return new WaitForSeconds(timeBetweenSpawns);
            SpawnEnemy(enemyPrefab, gameObject.transform.position);
        }

    }

    public void SpawnEnemy(GameObject gameObject, Vector3 spawnPosition)
    {
        // check if we have set parent trasform
        Transform parentTransform = parentObjectForEnemies != null ? parentObjectForEnemies : transform;

        if (gameObject != null)
        {
            GameObject enemyGameObject =
                    Instantiate(
                        gameObject,
                        spawnPosition,
                        Quaternion.identity, // Spawn the enemy facing the player
                        parentTransform);

            enemyGameObject.GetComponent<Enemy>().Init();
        }
    }

}
