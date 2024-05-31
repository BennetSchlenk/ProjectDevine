using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(EnemyMovementController))]
public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Basic configs")]    
    [SerializeField] private EnemyClassSO classAndStats;

    public float Health { get; set; }
    public float Armor { get; set; }
    public EnemyClassSO Stats { get {  return classAndStats; } }
    public EnemyMovementController MovementController {  get; private set; }


    private Dictionary<DamageTypeSO, DamageData> infectiousDamageTypes = new();
    private Dictionary<DamageTypeSO, (DamageData Data, float LastTick, float StopTime, IXPGainer XpGainder)> activeDamageOverTime = new();

    //cached vars
    AudioManager audioManager;
    PlayerDataManager playerDataManager;

    #region Unity Callbacks

    private void Awake()
    {
        Health = classAndStats.InitialLife;
        Armor = classAndStats.InitialArmor;
        MovementController = GetComponent<EnemyMovementController>();
    }

    private void Start()
    {
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        playerDataManager = ServiceLocator.Instance.GetService<PlayerDataManager>();
    }

    private void Update()
    {
        HandleDamageOverTime();
    }

    #endregion

    public void Init(List<Vector3> waypoints)
    {        
        MovementController.StartMoving(waypoints);
    }

    /// <summary>
    /// In case of Instant Kill player will not get Essence for the kill.
    /// </summary>
    public void InstantKill()
    {
        DestroySelf(false);
    }

    public void TakeDamage(List<DamageData> damageDataList, IXPGainer xpGainer)
    {
        
        foreach (DamageData damageData in damageDataList)
        {
            
            if (damageData.Damage > 0)
            {
                float damageTaken = HandleHealthDamage(damageData.Damage);
                Debug.Log($"<b>Enemy</b><color=#E60000> DirectDamage {damageTaken} {damageData.DamageType.DamageTypeName.ToUpper()} damage</color>");
                if (xpGainer != null)
                {
                    xpGainer.OnXPGain(damageTaken);
                }
            }

            if (damageData.DamageOverTime > 0)
            {
                // adding Infectious damages to separate dictionary for easier tracking
                if (damageData.DamageType.IsInfection)
                {
                    infectiousDamageTypes.TryAdd(damageData.DamageType, damageData);
                }

                // check if damageOverTime is already ticking
                if (!activeDamageOverTime.ContainsKey(damageData.DamageType))
                {
                    // adding it
                    activeDamageOverTime.Add(damageData.DamageType,
                        (damageData,
                        Time.time, 
                        Time.time + damageData.DamageOverTimeDuration,
                        xpGainer));
                }
                else
                {
                    // extending duration and updating tower reference
                    activeDamageOverTime[damageData.DamageType] = 
                        (damageData, 
                        activeDamageOverTime[damageData.DamageType].LastTick,
                        Time.time + damageData.DamageOverTimeDuration, 
                        xpGainer);
                }

            }
        }
    }

    private void HandleDamageOverTime()
    {
        List<DamageTypeSO> expiredDamageTypes = new();

        foreach (var details in activeDamageOverTime)
        {
            float timeSinceLastTick = Time.time - details.Value.LastTick;

            // checking if we reached or passed stop time
            if (details.Value.StopTime <= Time.time)
            {
                expiredDamageTypes.Add(details.Key);
            }
            else
            {
                if (timeSinceLastTick >= details.Value.Data.DamageOverTimeTickRate)
                {
                    
                    float damageTaken = HandleHealthDamage(details.Value.Data.DamageOverTime);

                    Debug.Log($"<b>Enemy</b><color=#FFB800> DOT Damage {damageTaken} {details.Key.DamageTypeName.ToUpper()} damage</color>");

                    if (damageTaken > 0)
                    {
                        // add effect
                        if (details.Key.DamageOverTimeEffect != null)
                        {
                            GameObject newHitEffect = Instantiate(details.Key.DamageOverTimeEffect, transform.position, Quaternion.identity);
                            newHitEffect.transform.SetParent(transform);
                        }
                    }

                    // add XP to tower
                    if (details.Value.XpGainder != null)
                    {
                        details.Value.XpGainder.OnXPGain(damageTaken);
                    }


                }
            }
        }

        foreach (var item in expiredDamageTypes)
        {
            activeDamageOverTime.Remove(item);
            infectiousDamageTypes.Remove(item);
        }
    }


    private float HandleArmorDamage(float amount)
    {
        float armorDamageTaken = 0f;

        if (Armor > 0)
        {
            float newArmor = Mathf.Clamp(Armor - amount, 0f, Armor);

            armorDamageTaken = Armor - newArmor;
            Armor = newArmor;

            if (armorDamageTaken > 0f)
            {
                // Handle the armor damage taken, animations, effects, etc
                        
            }
            
        }

        return armorDamageTaken;
        
    }

    private float HandleHealthDamage(float amount)
    {
        float damageTaken = 0f;
        float newHealth;

        float reducedDamageBasedOnArmor = amount;
        if (Armor > 0)
        {
            reducedDamageBasedOnArmor = Mathf.Clamp(Armor - amount, 0, amount);
        }        

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
        DestroySelf(true);
    }

    private void DestroySelf(bool shouldGetEssencePoints)
    {
        if (shouldGetEssencePoints)
        {
            playerDataManager.AddEssence(classAndStats.PointsForPlayerIfKilled);
        }

        StopAllCoroutines();
        Destroy(gameObject);
    }

}
