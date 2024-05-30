using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemySpawner : MonoBehaviour
{
    //[Header("For Testing")]
    //[SerializeField] private GameObject enemyPrefab;
    //[SerializeField] private float timeBetweenSpawns = 2f;
    //[SerializeField] private int maxSpawnCount = 20;

    [SerializeField, Space] private Transform parentObjectForEnemies;
    
    private WaypointsContainer _waypointsContainer;
    private EnemyWavesSO wavesSO;

    private Coroutine spawnLoopCR;
    private int loopCount = 0;

    #region Unity Callbacks

    private void Awake()
    {
        parentObjectForEnemies = GameObject.Find("Enemies").transform;
        wavesSO = Resources.Load(GlobalData.DefaultEnemyWaves) as EnemyWavesSO;
    }

    private void Start()
    {
        // get waypoints from spawner
        _waypointsContainer = GetComponent<WaypointsContainer>();

        
        spawnLoopCR = StartCoroutine(HandleWaves());
    }

    #endregion


    private IEnumerator HandleWaves()
    {
        // wait to start waves
        yield return new WaitForSeconds(wavesSO.InitialWaitTime);    

        while (true)
        {
        
            // loop through enemy waves
            foreach (var wave in wavesSO.Waves)
            {
                // wait before this wave
                yield return new WaitForSeconds(wave.WaitBeforeStartingThisWave);

                // loop through the current wave
                for (int i = 0; i < wave.HowManyInTheWave; i++)
                {
                    SpawnEnemy(wave.Enemy.gameObject, _waypointsContainer.WaypointsList);
                    yield return new WaitForSeconds(wave.SpawnInterval);
                }
            }

            loopCount++;

            if (loopCount == 10000)
            {
                Debug.LogWarning("You are crazy to allow to run 10000 spawn cycles");
                break;
            }
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
                        _waypointsContainer.transform.rotation, // Spawn the enemy facing the player
                        parentTransform);

            enemyGameObject.GetComponent<Enemy>().Init(waypoints);
        }
    }

}
