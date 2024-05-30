using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TowerAttackHandler : MonoBehaviour, IXPGainer
{
    public UnityEvent<Transform> OnTargetChange;

    [SerializeField] private AttackTargetType attackTargetType;
    [SerializeField] private GameObject projectile;
    [SerializeField] private List<ProjectileSpawnPoint> projectileSpawnPoints;

    private Transform enemiesContainerTransform;
    private int currentProjectileSpawnPointIndex = 0;
    private List<FaceTarget> faceTargets = new();

    private void Start()
    {
        enemiesContainerTransform = GameObject.Find("Enemies").transform;
        if (enemiesContainerTransform == null)
            Debug.LogError("Enemies container not found.");

        StoreAllFaceTargetInChildren();
        StoreAllProjectileSpawnPoints();
    }

    /// <summary>
    /// Perform an attack with the tower data and damage data list.
    /// </summary>
    /// <param name="towerData"></param>
    /// <param name="damageDataList"></param>
    /// <returns>Enemy GameObject</returns>
    public GameObject Attack(TowerRuntimeStats towerData, List<DamageData> damageDataList, List<GameObject> enemiesToIgnoreIfPossible)
    {
        var enemy = GetAttackTargetWithinRange(attackTargetType, towerData.Range, enemiesToIgnoreIfPossible);
        if (enemy == null)
        {
            return null;
        }
        else
        {
            if (IsEnemyInRange(enemy, towerData.Range))
            {
                // Get the damagable component of the enemy
                IDamagable damagable = enemy.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    OnTargetChange.Invoke(enemy.transform);
                    SetAllFaceTargets(enemy);
                    return SpawnAndConfigureProjectile(towerData, damageDataList, enemy, damagable);
                }
            }
            else
            {
                Debug.Log("Enemy is out of range.");
                return null;
            }
            return null;
        }
    }

    public void StoreAllFaceTargetInChildren()
    {
        faceTargets.Clear();
        foreach (FaceTarget faceTarget in GetComponentsInChildren<FaceTarget>())
            faceTargets.Add(faceTarget);
    }

    public void StoreAllProjectileSpawnPoints()
    {
        projectileSpawnPoints.Clear();
        foreach (ProjectileSpawnPoint projectileSpawnPoint in GetComponentsInChildren<ProjectileSpawnPoint>())
        {
            projectileSpawnPoints.Add(projectileSpawnPoint);
        }

        // Sort the projectile spawn points based on their priority, less is first
        projectileSpawnPoints.Sort((a, b) => a.GetComponent<ProjectileSpawnPoint>().Priority.CompareTo(b.GetComponent<ProjectileSpawnPoint>().Priority));
    }

    public void OnXPGain(float xp)
    {
        Debug.Log("XP Gained: " + xp);
        // TODO: Implement
    }

    private GameObject SpawnAndConfigureProjectile(TowerRuntimeStats towerData, List<DamageData> damageDataList, GameObject enemy, IDamagable damagable)
    {
        if (projectileSpawnPoints != null && projectileSpawnPoints.Count > 0)
        {
            // Spawn projectile. TODO: Pooling
            GameObject newProjectile = Instantiate(projectile, projectileSpawnPoints[currentProjectileSpawnPointIndex].transform.position, Quaternion.identity);
            currentProjectileSpawnPointIndex = (currentProjectileSpawnPointIndex + 1) % projectileSpawnPoints.Count;
            Projectile projectileComponent = newProjectile.GetComponent<Projectile>();
            List<GameObject> effects = new();
            List<GameObject> hitEffects = new();
            foreach (DamageData damageData in damageDataList)
            {
                effects.Add(damageData.DamageType.TrailEffect);
                hitEffects.Add(damageData.DamageType.HitEffect);
            }

            projectileComponent.MoveTowardsTarget(enemy.transform, towerData.ProjectileSpeed, effects, hitEffects, () =>
            {
                DealDamage(damagable, damageDataList);
            });

            return enemy;


        }
        else
        {
            Debug.LogWarning("Projectile spawn points not set.");
            return null;
        }
    }

    private void SetAllFaceTargets(GameObject enemy)
    {
        foreach (FaceTarget faceTarget in faceTargets)
            faceTarget.SetTarget(enemy.transform);
    }

    private GameObject GetAttackTargetWithinRange(AttackTargetType attackTargetType, float range, List<GameObject> enemiesToIgnoreIfPossible)
    {
        // TODO: Integrate with IDamageable interface.
        // Get the list of enemies
        List<GameObject> allEnemies = new List<GameObject>();
        foreach (Transform enemy in enemiesContainerTransform)
            allEnemies.Add(enemy.gameObject);

        if (allEnemies.Count == 0)
            return null;

        List<GameObject> potentialEnemies = new List<GameObject>();


        // Get the target based on the attack target type
        switch (attackTargetType)
        {
            case AttackTargetType.Closest:
                potentialEnemies = GetClosestEnemiesWithinRange(allEnemies, range);
                break;
            case AttackTargetType.Farthest:
                potentialEnemies = GetFarthestEnemiesWithinRange(allEnemies, range);
                break;
            case AttackTargetType.Strongest:
                potentialEnemies = GetStrongestEnemyWithinRange(allEnemies, range);
                break;
            case AttackTargetType.Weakest:
                potentialEnemies = GetWeakestEnemiesWithinRange(allEnemies, range);
                break;
            default:
                Debug.LogError("Attack target type not found.");
                return null;
                break;
        }

        // If there are no potential enemies, return null
        if (potentialEnemies.Count == 0)
            return null;
        // If there are potential enemies, try to ignore the enemies in the enemiesToIgnoreIfPossible list
        else
        {
            foreach (GameObject potentialEnemy in potentialEnemies)
            {
                if (!enemiesToIgnoreIfPossible.Contains(potentialEnemy))
                    return potentialEnemy;
            }
        }

        return null;
    }

    private List<GameObject> GetClosestEnemiesWithinRange(List<GameObject> enemies, float range)
    {
        List<GameObject> closestEnemies = new();
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (IsEnemyInRange(enemy, range))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestEnemies.Add(enemy);
                }
            }
        }

        // Sort the closest enemies based on their distance
        closestEnemies.Sort((a, b) => Vector3.Distance(transform.position, a.transform.position).CompareTo(Vector3.Distance(transform.position, b.transform.position)));

        return closestEnemies;
    }
    private List<GameObject> GetFarthestEnemiesWithinRange(List<GameObject> enemies, float range)
    {
        List<GameObject> farthestEnemies = new();

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= range)
            {
                farthestEnemies.Add(enemy);
            }
        }

        // Sort the farthest enemies based on their distance
        farthestEnemies.Sort((a, b) => Vector3.Distance(transform.position, b.transform.position).CompareTo(Vector3.Distance(transform.position, a.transform.position)));

        return farthestEnemies;
    }
    private List<GameObject> GetStrongestEnemyWithinRange(List<GameObject> enemies, float range)
    {
        List<GameObject> strongestEnemies = new();

        foreach (GameObject enemy in enemies)
        {
            if (IsEnemyInRange(enemy, range))
            {
                IDamagable damagable = enemy.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    strongestEnemies.Add(enemy);
                }
            }
        }

        // Sort the strongest enemies based on their health
        strongestEnemies.Sort((a, b) => b.GetComponent<IDamagable>().Health.CompareTo(a.GetComponent<IDamagable>().Health));    

        return strongestEnemies;
    }
    private List<GameObject> GetWeakestEnemiesWithinRange(List<GameObject> enemies, float range)
    {
        List<GameObject> weakestEnemies = new();

        foreach (GameObject enemy in enemies)
        {
            if (IsEnemyInRange(enemy, range))
            {
                IDamagable damagable = enemy.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    weakestEnemies.Add(enemy);
                }
            }
        }

        // Sort the weakest enemies based on their health
        weakestEnemies.Sort((a, b) => a.GetComponent<IDamagable>().Health.CompareTo(b.GetComponent<IDamagable>().Health));

        return weakestEnemies;
    }

    private bool IsEnemyInRange(GameObject enemy, float range)
    {
        return Vector3.Distance(transform.position, enemy.transform.position) <= range;
    }

    private void DealDamage(IDamagable damagable, List<DamageData> damageDataList)
    {
        //damagable.TakeDamage(damageDataList, this);
    }

}





public enum AttackTargetType
{
    Closest,
    Farthest,
    Strongest,
    Weakest
}