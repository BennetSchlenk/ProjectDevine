using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAttackHandler : MonoBehaviour
{
    public GameObject TemporalEnemiesParent;

    [SerializeField] private AttackTargetType attackTargetType;

    public void Attack(TowerData towerData, List<DamageData> damageDataList)
    {
        var enemy = GetAttackTarget(attackTargetType);
        if (enemy == null)
            Debug.Log("No enemy found to attack.");
        else
        {
            if (IsEnemyInRange(enemy, towerData.Range))
            {
                // Attack the enemy
                Debug.Log("Attacking enemy " + enemy.gameObject.name);
            }
            else
                Debug.Log("Enemy is out of range.");
        }
    }

    private GameObject GetAttackTarget(AttackTargetType attackTargetType)
    {
        // TODO: Integrate with IDamageable interface.
        // Get the list of enemies
        List<GameObject> enemies = new List<GameObject>();
        foreach (Transform enemy in TemporalEnemiesParent.transform)
            enemies.Add(enemy.gameObject);

        // If there are no enemies, return null
        if (enemies.Count == 0)
            return null;

        // Get the target based on the attack target type
        switch (attackTargetType)
        {
            case AttackTargetType.Closest:
                return GetClosestEnemy(enemies);
            case AttackTargetType.Farthest:
                //return GetFarthestEnemy(enemies);
                return null;
            case AttackTargetType.Strongest:
                //return GetStrongestEnemy(enemies);
                return null;
            case AttackTargetType.Weakest:
                //return GetWeakestEnemy(enemies);
                return null;
        }

        return null;
    }

    private GameObject GetClosestEnemy(List<GameObject> enemies)
    {
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    private bool IsEnemyInRange(GameObject enemy, float range)
    {
        return Vector3.Distance(transform.position, enemy.transform.position) <= range;
    }
}



public enum AttackTargetType
{
    Closest,
    Farthest,
    Strongest,
    Weakest
}