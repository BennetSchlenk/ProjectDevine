using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyHealthBarUI : MonoBehaviour
{
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform armorBar;
    [SerializeField] private RectTransform armorBackgroundBar;
    [SerializeField] private Vector3 offset;  // The offset from the target's position

    private Enemy enemy;

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (enemy == null)
        {
            return;
        }

        // Convert the target's world position to screen space
        Vector3 screenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);

        // Set the position of the UI element to the screen position
        transform.position = screenPos + offset;
    }

    private void OnDisable()
    {
        enemy.OnHealthChanged -= OnHealthChange;
        enemy.OnEnemyDied -= OnEnemyDied;
        enemy.OnArmorChanged -= OnArmorChange;
    }

    #endregion


    public void SetEnemy(Enemy enemy)
    {
        this.enemy = enemy;
        enemy.OnHealthChanged += OnHealthChange;
        enemy.OnEnemyDied += OnEnemyDied;
        enemy.OnArmorChanged += OnArmorChange;
    }

    private void OnHealthChange(float newHealth, float maxHealth)
    {
        healthBar.localScale = new Vector3(newHealth / maxHealth, 1, 1);
    }

    private void OnArmorChange(float newArmor, float maxArmor)
    {
        armorBar.localScale = new Vector3(newArmor / maxArmor, 1, 1);
        if (armorBar.gameObject.activeSelf == false)
        {
            armorBar.gameObject.SetActive(true);
            armorBackgroundBar.gameObject.SetActive(true);
        }

        if (newArmor <= 0)
        {
            armorBar.gameObject.SetActive(false);
            armorBackgroundBar.gameObject.SetActive(false);
        }
    }

    private void OnEnemyDied(Enemy enemy)
    {
        Debug.Log("Enemy Died");
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        Destroy(gameObject);
    }

    
}
