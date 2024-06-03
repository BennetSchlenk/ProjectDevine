using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(EnemyMovementController))]
public class Enemy : MonoBehaviour, IDamagable
{
    [Header("Basic configs")]    
    [SerializeField] private EnemyClassSO classAndStats;
    [SerializeField] private float raycastLength = 0.1f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Collider baseCollider;

    [Header("VFX")]
    [SerializeField] private Transform VFXSpawnPoint;
    public Transform VFXSpawnPointProp => VFXSpawnPoint;

    public float Health { get; set; }
    public float Armor { get; set; }
    public float MovementSpeed { get { return classAndStats.MovementSpeed * speedModifier * difficultyMultiplier; } }
    public float RotationSpeed { get { return classAndStats.MovementSpeed * speedModifier * difficultyMultiplier * 100f; } }
    public EnemyClassSO Stats { get {  return classAndStats; } }
    public EnemyMovementController MovementController {  get; private set; }


    private Dictionary<DamageTypeSO, DamageData> infectiousDamageTypes = new();
    private Dictionary<DamageTypeSO, (DamageData Data, float LastTick, float StopTime, IXPGainer XpGainder)> activeDamageOverTime = new();
    private Dictionary<DamageTypeSO, GameObject> activeVFXObjects = new();

    private float speedModifier = 1f;
    private float difficultyMultiplier = 1f;
    private Tween hitTween;

    //cached vars
    AudioManager audioManager;
    PlayerDataManager playerDataManager;

    #region Enemy Health Bar Integration

    public event Action<float, float> OnHealthChanged = delegate { };
    public event Action<float, float> OnArmorChanged = delegate { };
    public event Action<Enemy> OnEnemyDied = delegate { };

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        Health = classAndStats.InitialLife * difficultyMultiplier;
        Armor = classAndStats.InitialArmor * difficultyMultiplier;
        MovementController = GetComponent<EnemyMovementController>();

        if (baseCollider == null)
        {
            baseCollider = GetComponent<Collider>();
            if (baseCollider == null) baseCollider = GetComponentInChildren<Collider>();            
        }

        if (VFXSpawnPoint == null)
        {
            VFXSpawnPoint = transform;
        }

    }

    private void Start()
    {
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();
        playerDataManager = ServiceLocator.Instance.GetService<PlayerDataManager>();

        hitTween = transform.DOScale(1.05f, 0.1f).SetEase(Ease.InOutCubic).SetLoops(2, LoopType.Yoyo).SetAutoKill(false);
    }

    private void Update()
    {
        HandleDamageOverTime();
        CheckForSpeedModifiers();
    }

    private void FixedUpdate()
    {
        CheckForEnemies();
    }

    #endregion

    public void Init(List<Vector3> waypoints, float difficultyMultiplier)
    {        
        this.difficultyMultiplier = difficultyMultiplier;
        MovementController.StartMoving(waypoints);
    }

    public void TakeDamage(List<DamageData> damageDataList, IXPGainer xpGainer)
    {
        
        foreach (DamageData damageData in damageDataList)
        {
            
            if (damageData.Damage > 0)
            {
                float damageTaken = HandleHealthDamage(damageData.Damage);
                //Debug.Log($"<b>Enemy</b><color=#E60000> DirectDamage {damageTaken} {damageData.DamageType.DamageTypeName.ToUpper()} damage</color>");
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
        List<DamageTypeSO> dealtDamageList = new();

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

                    //Debug.Log($"<b>Enemy</b><color=#FFB800> DOT Damage {damageTaken} {details.Key.DamageTypeName.ToUpper()} damage</color>");

                    if (damageTaken > 0)
                    {
                        // add to modify this list
                        dealtDamageList.Add(details.Key);

                        // add effect if not yet added
                        if (details.Key.DamageOverTimeEffect != null 
                            && !activeVFXObjects.ContainsKey(details.Key))
                        {
                            GameObject newHitEffect = Instantiate(details.Key.DamageOverTimeEffect, VFXSpawnPoint.position, Quaternion.identity);
                            newHitEffect.transform.SetParent(transform);

                            activeVFXObjects.Add(details.Key, newHitEffect);
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

        // update last tick for every damagetype what dealt damage
        foreach (var dealtDamage in dealtDamageList)
        {
            activeDamageOverTime[dealtDamage] =
                        (activeDamageOverTime[dealtDamage].Data,
                        Time.time,
                        activeDamageOverTime[dealtDamage].StopTime,
                        activeDamageOverTime[dealtDamage].XpGainder);
        }


        foreach (var item in expiredDamageTypes)
        {
            activeDamageOverTime.Remove(item);
            infectiousDamageTypes.Remove(item);
            activeVFXObjects.Remove(item);
        }
    }

    private void CheckForSpeedModifiers()
    {
        List<float> speedModifiers = new();

        foreach (var details in activeDamageOverTime)
        {
            float damageDataSpeedModifier = details.Value.Data.SpeedMultiplier;
            speedModifiers.Add(damageDataSpeedModifier);
        }

        if (speedModifiers.Count > 0)
        {
            speedModifiers.Sort();
            speedModifier = speedModifiers[0];
        }
        else
        {
            speedModifier = 1f;
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

                #region Enemy Health Bar Integration

                OnArmorChanged.Invoke(Armor, classAndStats.InitialArmor * difficultyMultiplier);

                #endregion
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

            #region Enemy Health Bar Integration

            OnHealthChanged.Invoke(newHealth, classAndStats.InitialLife * difficultyMultiplier);

            #endregion

            damageTaken = Health - newHealth;
            Health = newHealth;

            if (damageTaken > 0f)
            {
                // Handle the damage taken, animations, effects, etc
                if (hitTween != null && !hitTween.IsPlaying())
                    hitTween.Restart();


                // trigger death of enemy
                if (Health <= 0f)
                {
                    DestroySelf();
                }
            }
        } 
        return damageTaken;
    }

    private void CheckForEnemies()
    {
        //Debug.Log($"CheckForEnemies()");

        if (infectiousDamageTypes.Count > 0)
        {
            // switching off own collider temporarily to avoid self-collision
            baseCollider.enabled = false;
                        
            Ray ray = new Ray(transform.position, transform.forward);

            // Perform raycast and store the hit information
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastLength, enemyLayer))
            {
                //Debug.Log($"CheckForEnemies() - Physics.Raycast HIT");

                if (!hit.collider.gameObject.TryGetComponent(out Enemy enemy))
                {
                    enemy = hit.collider.gameObject.GetComponentInParent<Enemy>();
                }

                if (enemy != null)
                {
                    //Debug.Log($"CheckForEnemies() - enemy {enemy.gameObject.name}");

                    List<DamageData> damageDataList = new();

                    foreach (var infectiousDamage in infectiousDamageTypes)
                    {
                        damageDataList.Add(infectiousDamage.Value);
                        //Debug.Log($"Enemy infecting other enemy with Damage: {infectiousDamage.Key.DamageTypeName}");
                    }

                    // passing in the list of infecting damages and NULL as no points should be given for these damages
                    enemy.TakeDamage(damageDataList, null);
                }
            }

            baseCollider.enabled = true;
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

    #region Self Destruct related functions

    /// <summary>
    /// In case of Instant Kill player will not get Essence for the kill.
    /// </summary>
    public void InstantKill()
    {
        DestroySelf(false);
    }

    private void DestroySelf()
    {
        DestroySelf(true);

    }

    private void DestroySelf(bool shouldGetEssencePoints)
    {
        #region Enemy Health Bar Integration
        //Debug.Log("Enemy Destroyed");
        OnEnemyDied.Invoke(this);
        GlobalData.EnemiesLeftCount--;
        #endregion

        if (shouldGetEssencePoints)
        {
            playerDataManager.AddEssence(classAndStats.PointsForPlayerIfKilled);
        }

        audioManager.PlaySFXOneShotAtPosition("enemyDied", transform.position);

        StopAllCoroutines();
        Destroy(gameObject);
    }

    #endregion

}
