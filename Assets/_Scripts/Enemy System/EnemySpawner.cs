using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemySpawner : MonoBehaviour
{
    [Header("For Testing")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private int maxSpawnCount = 20;

    [SerializeField, Space] private Transform parentObjectForEnemies;


    public static EnemySpawner Instance;

    private Waypoints waypoints;
    
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

        parentObjectForEnemies = GameObject.Find("Enemies").transform;
    }

    private void Start()
    {
        // get waypoints from spawner
        waypoints = GetComponent<Waypoints>();

        StartCoroutine(SpawnLoop());
    }

    #endregion

    private IEnumerator SpawnLoop()
    {
        for (int i = 0; i < maxSpawnCount; i++)
        {            
            yield return new WaitForSeconds(timeBetweenSpawns);
            SpawnEnemy(enemyPrefab, waypoints.WaypointsList);
        }

    }

    public void SpawnEnemy(GameObject gameObject, List<Vector3> waypoints)
    {
        // check if we have set parent trasform
        Transform parentTransform = parentObjectForEnemies != null ? parentObjectForEnemies : transform;

        Assert.IsTrue(waypoints.Count > 0);
        Vector3 spawnPosition = waypoints[0];

        if (gameObject != null)
        {
            GameObject enemyGameObject =
                    Instantiate(
                        gameObject,
                        spawnPosition,
                        Quaternion.identity, // Spawn the enemy facing the player
                        parentTransform);

            enemyGameObject.GetComponent<Enemy>().Init(waypoints);
        }
    }

}
