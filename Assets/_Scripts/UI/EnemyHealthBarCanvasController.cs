using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBarCanvasController : MonoBehaviour
{
    [SerializeField] private GameObject enemyHealthBarPrefab;
    [SerializeField] private Transform parentObjectForHealthBars;

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        GlobalData.OnEnemySpawned += OnEnemySpawned;
    }

    #endregion


    private void OnEnemySpawned(Enemy enemy)
    {
        GameObject healthBar = Instantiate(enemyHealthBarPrefab, parentObjectForHealthBars);
        // Position the health bar out of the screen
        healthBar.transform.position = new Vector3(-10000, -1000, 0);

        healthBar.GetComponent<EnemyHealthBarUI>().SetEnemy(enemy);
    }
}
