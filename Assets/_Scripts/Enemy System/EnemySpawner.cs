using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemySpawner : MonoBehaviour
{    
    [SerializeField, Space] private Transform parentObjectForEnemies;
    
    private WaypointsContainer _waypointsContainer;
    private EnemyWavesSO wavesSO;

    private Coroutine spawnLoopCR;
    private int loopCount = 0;
    private BasePool[] pools;
    private int currentWave = 0;

    private float difficultyMultiplier = 1f;

    private const float DEFAULT_DIFFICULTY_MULTIPLIER = 1f;
    private const float DIFFICULTY_MULTIPLIER_INCREASE = 1.6f;

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

        difficultyMultiplier = DEFAULT_DIFFICULTY_MULTIPLIER;

        pools = GetComponents<BasePool>();
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
                currentWave++;
                Debug.Log("Waiting before starting wave: " + wave.WaitBeforeStartingThisWave);
                // wait before this wave
                //yield return new WaitForSeconds(wave.WaitBeforeStartingThisWave);

                for (int i = 0; i < wave.WaitBeforeStartingThisWave; i++)
                {
                    GlobalData.OnChangeWaveMessage?.Invoke("Wave " + (currentWave) + " starts in " + (wave.WaitBeforeStartingThisWave - i) + " seconds");
                    yield return new WaitForSeconds(1f);
                }

                GlobalData.OnChangeWaveMessage?.Invoke("Wave " + currentWave);

                int enemiesLeft = wave.HowManyInTheWave;
                GlobalData.EnemiesLeftCount += enemiesLeft;

                // loop through the current wave
                for (int i = 0; i < wave.HowManyInTheWave; i++)
                {
                    SpawnEnemy(wave.Enemy.gameObject, _waypointsContainer.WaypointsList);
                    yield return new WaitForSeconds(wave.SpawnInterval);
                }

                while (GlobalData.EnemiesLeftCount > 0)
                {
                    Debug.Log("Enemies left: " + GlobalData.EnemiesLeftCount);
                    yield return new WaitForSeconds(1f);
                }

                Debug.Log("SUBWAVE FINISHED");
            }

            // TODO: Show wave finished message
            GlobalData.OnChangeWaveMessage?.Invoke("Wave " + (currentWave) + " finished");

            loopCount++;

            difficultyMultiplier *= DIFFICULTY_MULTIPLIER_INCREASE;

            if (loopCount == 10000)
            {
                Debug.LogError("You are crazy to allow to run 10000 spawn cycles");
                ServiceLocator.Instance.GetService<GameManager>().SetGameState(GameStates.GameOver);
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
            //GameObject enemyGameObject =
            //        Instantiate(
            //            gameObject,
            //            spawnPosition,
            //            _waypointsContainer.transform.rotation, // Spawn the enemy facing the player
            //            parentTransform);

            GameObject enemyGameObject = GetPoolByGameObject(gameObject).pool.Get().gameObject;
            enemyGameObject.transform.position = spawnPosition;
            enemyGameObject.transform.rotation = _waypointsContainer.transform.rotation;
            enemyGameObject.transform.SetParent(parentTransform);

            var enemy = enemyGameObject.GetComponent<Enemy>();
            enemy.Init(waypoints, difficultyMultiplier);

            #region Enemy Health Bar Integration

            GlobalData.OnEnemySpawned?.Invoke(enemy);

            #endregion
        }
    }

    private BasePool GetPoolByGameObject(GameObject go)
    {
        // Return the pool from pools where the objectToPool is the same as the GameObject passed as parameter
        var pool = Array.Find(pools, x => x.ObjectToPool.gameObject == go);

        if (pool == null)
            Debug.LogError("Pool not found for GameObject: " + go.name);

        return pool;
    }

}
