using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TowerAttackHandler : MonoBehaviour
{
    public UnityEvent<Transform> OnTargetChange;

    [SerializeField] private AttackTargetType attackTargetType;
    [SerializeField] private GameObject projectile;
    [SerializeField] private List<Transform> projectileSpawnPoints;

    private Transform enemiesContainerTransform;
    private int currentProjectileSpawnPointIndex = 0;

    private void Start()
    {
        enemiesContainerTransform = GameObject.Find("Enemies").transform;
        if (enemiesContainerTransform == null)
            Debug.LogError("Enemies container not found.");
    }

    public void Attack(TowerData towerData, List<DamageData> damageDataList)
    {
        var enemy = GetAttackTargetWithinRange(attackTargetType, towerData.Range);
        if (enemy == null)
            Debug.Log("No enemy found to attack.");
        else
        {
            if (IsEnemyInRange(enemy, towerData.Range))
            {
                // Get the damagable component of the enemy
                IDamagable damagable = enemy.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    OnTargetChange.Invoke(enemy.transform);

                    // Spawn projectile. TODO: Pooling
                    GameObject newProjectile = Instantiate(projectile, projectileSpawnPoints[currentProjectileSpawnPointIndex].position, Quaternion.identity);
                    currentProjectileSpawnPointIndex = (currentProjectileSpawnPointIndex + 1) % projectileSpawnPoints.Count;
                    Projectile projectileComponent = newProjectile.GetComponent<Projectile>();
                    List<GameObject> effects = new();
                    List<GameObject> hitEffects = new();
                    foreach (DamageData damageData in damageDataList)
                    {
                        effects.Add(damageData.DamageType.TrailEffect);
                        hitEffects.Add(damageData.DamageType.HitEffect);
                    }

                    projectileComponent.MoveTowardsTarget(enemy.transform, towerData.ProjectileSpeed, effects, hitEffects, () => {
                        DealDamage(damagable, damageDataList);
                    });


                    
                }
            }
            else
                Debug.Log("Enemy is out of range.");
        }
    }

    private GameObject GetAttackTargetWithinRange(AttackTargetType attackTargetType, float range)
    {
        // TODO: Integrate with IDamageable interface.
        // Get the list of enemies
        List<GameObject> enemies = new List<GameObject>();
        foreach (Transform enemy in enemiesContainerTransform)
            enemies.Add(enemy.gameObject);

        // If there are no enemies, return null
        if (enemies.Count == 0)
            return null;

        // Get the target based on the attack target type
        switch (attackTargetType)
        {
            case AttackTargetType.Closest:
                return GetClosestEnemyWithinRange(enemies, range);
            case AttackTargetType.Farthest:
                return GetFarthestEnemyWithinRange(enemies, range);
            case AttackTargetType.Strongest:
                return GetStrongestEnemyWithinRange(enemies, range);
            case AttackTargetType.Weakest:
                return GetWeakestEnemyWithinRange(enemies, range);
            default:
                Debug.LogError("Attack target type not found.");
                return null;
                break;
        }

        return null;
    }

    private GameObject GetClosestEnemyWithinRange(List<GameObject> enemies, float range)
    {
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (IsEnemyInRange(enemy, range))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }

        return closestEnemy;
    }
    private GameObject GetFarthestEnemyWithinRange(List<GameObject> enemies, float range)
    {
        GameObject farthestEnemy = null;
        float farthestDistance = 0f;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > farthestDistance && distance <= range)
            {
                farthestDistance = distance;
                farthestEnemy = enemy;
            }
        }

        return farthestEnemy;
    }
    private GameObject GetStrongestEnemyWithinRange(List<GameObject> enemies, float range)
    {
        GameObject strongestEnemy = null;
        float highestHealth = 0f;

        foreach (GameObject enemy in enemies)
        {
            if (IsEnemyInRange(enemy, range))
            {
                IDamagable damagable = enemy.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    if (damagable.Health > highestHealth)
                    {
                        highestHealth = damagable.Health;
                        strongestEnemy = enemy;
                    }
                }
            }
        }

        return strongestEnemy;
    }
    private GameObject GetWeakestEnemyWithinRange(List<GameObject> enemies, float range)
    {
        GameObject weakestEnemy = null;
        float lowestHealth = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (IsEnemyInRange(enemy, range))
            {
                IDamagable damagable = enemy.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    if (damagable.Health < lowestHealth)
                    {
                        Debug.Log("Weakest enemy found: " + enemy.name);
                        lowestHealth = damagable.Health;
                        weakestEnemy = enemy;
                    }
                }
            }
        }

        return weakestEnemy;
    }

    private bool IsEnemyInRange(GameObject enemy, float range)
    {
        return Vector3.Distance(transform.position, enemy.transform.position) <= range;
    }

    private void DealDamage(IDamagable damagable, List<DamageData> damageDataList)
    {
        foreach (DamageData damageData in damageDataList)
        {
            damagable.TakeDamage(damageData.Damage);
            Debug.LogFormat("Dealing {0} damage to {1} ", damageData.Damage, damagable);
        }
    }
}



public enum AttackTargetType
{
    Closest,
    Farthest,
    Strongest,
    Weakest
}