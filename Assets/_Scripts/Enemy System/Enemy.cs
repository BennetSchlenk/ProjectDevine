using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(EnemyMovementController))]
public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Basic configs")]    
    [SerializeField] private EnemyClassSO classAndStats;

    public float Health { get; set; }
    public float Armor { get; set; }
    public EnemyClassSO Stats { get {  return classAndStats; } }
    public EnemyMovementController MovementController {  get; private set; }

    public List<(IXPGainer XpGainder, DamageData DamageData)> infectiousDamageTypes = new();

    public List<(DamageTypeSO DamageType, DamageData DamageData, Coroutine damageOverTimeCR)> activeDamageOverTimeList = new();

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

    public void InstantKill()
    {
        DestroySelf();        
    }

    public void TakeDamage(List<DamageData> damageDataList, IXPGainer xpGainer)
    {
        foreach (DamageData damageData in damageDataList)
        {
            if (damageData.Damage > 0)
            {
                float damageTaken = HandleHealthDamage(damageData.Damage);
                if (xpGainer != null)
                {
                    xpGainer.OnXPGain(damageTaken);
                }
            }

            if (damageData.DamageOverTime > 0)
            {

                //if (damageData.DamageType.IsInfectious)
                //{

                //}

                StartCoroutine(
                    DealDamageOverTime(
                        damageData.DamageOverTime, 
                        damageData.DamageOverTimeTickRate, 
                        damageData.DamageOverTimeDuration, 
                        xpGainer));
            }
        }

       
        
    }


    private float HandleArmorDamage(float amount)
    {
        float armorDamageTaken = 0f;

        float newArmor = Mathf.Clamp(Armor - amount, 0f, Armor);

        armorDamageTaken = Armor - newArmor;
        Armor = newArmor;

        if (armorDamageTaken > 0f)
        {
            // Handle the armor damage taken, animations, effects, etc
                        
        }

        return armorDamageTaken;
        
    }

    private float HandleHealthDamage(float amount)
    {
        float damageTaken = 0f;
        float newHealth;
        float reducedDamageBasedOnArmor = Mathf.Clamp(Armor - amount, 0, amount);

        if (reducedDamageBasedOnArmor > 0)
        {
            newHealth = Mathf.Clamp(Health - reducedDamageBasedOnArmor, 0f, Health);

            damageTaken = Health - newHealth;
            Health = newHealth;

            if (damageTaken > 0f)
            {
                // Handle the damage taken, animations, effects, etc

                // trigger death of enemy
                if (Health <= 0f)
                {
                    DestroySelf();
                }
            }
        } 
        return damageTaken;
    }

    private IEnumerator DealDamageOverTime(float damageAmount, float damageInterval, float damageDuration, IXPGainer xpGainer)
    {
        float timeOfStart = 0f; 

        while (timeOfStart <= damageDuration)
        {
            float damageTaken = HandleHealthDamage(damageAmount);
            if (xpGainer != null)
            {
                xpGainer.OnXPGain(damageTaken);
            }

            timeOfStart += Time.deltaTime;
            yield return new WaitForSeconds(damageInterval);
        }

    }

    private void StartDamageOverTime(DamageData damageData)
    {
        foreach (var item in activeDamageOverTimeList)
        {
            //item.DamageType == damageData.DamageType;
        }
    }

    /// <summary>
    /// Returns the damage dealt to the Core
    /// </summary>
    /// <returns>the damage dealt to the Core</returns>
    public void ReachedCore(Core core)
    {
        core.DamageCore(classAndStats.CoreDamage);
        InstantKill();
    }

    private void DestroySelf()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }


}
