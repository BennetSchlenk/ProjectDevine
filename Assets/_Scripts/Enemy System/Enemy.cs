using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(EnemyMovementController))]
public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Basic configs")]    
    [SerializeField] private EnemyClassSO classAndStats;

    public float Health { get; set; }
    public EnemyClassSO Stats { get {  return classAndStats; } }
    public EnemyMovementController MovementController {  get; private set; }

    public List<DamageData> damageOverTimeList = new();

    private Coroutine dealDamageOverTimeCR;

    #region Unity Callbacks

    private void Awake()
    {
        Health = classAndStats.InitialLife;
        MovementController = GetComponent<EnemyMovementController>();
    }

    private void Update()
    {
        
    }

    #endregion

    public void Init(List<Vector3> waypoints)
    {        
        MovementController.StartMoving(waypoints);
    }

    public float InstantKill()
    {
        DestroySelf();
        return Health;
    }

    public void TakeDamage(List<DamageData> damageDataList, IXPGainer xpGainer)
    {
        List<(IDamagable, DamageData)> test; 

        foreach (DamageData damageData in damageDataList)
        {
            if (damageData.Damage > 0)
            {

            }
        }

       
        
    }

    private float TakeHealthDamage(float amount)
    {
        float damageTaken;

        Health = Mathf.Clamp(Health - amount, 0f, Health);

        damageTaken = Health - (Health - amount);

        if (damageTaken > 0f)
        {
            // Handle the damage taken, animations, effects, etc

            // trigger death of enemy
            if (Health <= 0f)
            {
                DestroySelf();
            }
        }

        return damageTaken;
    }

    /// <summary>
    /// Returns the damage dealt to the Core
    /// </summary>
    /// <returns>the damage dealt to the Core</returns>
    public void ReachedCore(Core core)
    {
        core.TakeDamage(classAndStats.CoreDamage);
        InstantKill();
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }


}
