using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour, IPlaceable, ISelectable
{

    [Header("Runtime Data")]
    public TowerInfoSO TowerInfo;
    [SerializeField] private TowerDataUpgradeSO baseTowerStats;
    [HideInInspector]
    public TowerRuntimeStats TowerRuntimeStats;
    public List<DamageData> DamageDataList = new(); // List of damage data for each damage type

    [SerializeField]
    private GameObject rangeGameObject;
    [Tooltip("The container for the tower model. The tower model will be instantiated here.")]
    [SerializeField] private Transform towerModelContainer;
    [SerializeField] private TowerAttackHandler attackHandler;

    [Header("Debug")]
    [SerializeField] private Color debugColor;

    private CardDataSO currentCardDataSO;
    private bool isWorking = false;
    private bool isFiring = false;
    private float fireCooldown = 0f;
    private GameObject currentModel;
    private float currentXP;

    public float CurrentXP => currentXP;
    public float CurrentLevelMaxXP
    {
        get
        {
            // If the tower is max level, return the current XP
            if (TowerRuntimeStats.Level > TowerInfo.TowerStatsPerLevel.Count)
                return currentXP;
            else
                return TowerInfo.TowerStatsPerLevel[TowerRuntimeStats.Level - 1].XP;
        }
    }

    public CardDataSO CurrentCardDataSO => currentCardDataSO;
    public int MaxTowerTier => TowerInfo.TowerModels.Count;
    public int MaxTowerModifiers => TowerRuntimeStats.Tier; // Temporal solution
    public int MaxLevel => CurrentCardDataSO.TowerInfo.TowerStatsPerLevel.Count + 1;

    #region Unity Callbacks

    private void Awake()
    {
        attackHandler.OnXPChange += OnXPChange;
    }

    private void OnDestroy()
    {
        attackHandler.OnXPChange -= OnXPChange;
    }

    private void Start()
    {

        if (TowerInfo == null)
            Debug.LogError("Tower info is not set!");

        if (attackHandler == null)
            attackHandler = GetComponent<TowerAttackHandler>();
        if (attackHandler == null)
            Debug.LogError("Attack handler is not set on the tower.");
        
    }

    private void Update()
    {
        if (!isWorking || isFiring) return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
            FireRound();
    }

    private void OnDrawGizmosSelected()
    {
        if (TowerRuntimeStats == null) return;

        Gizmos.color = debugColor;
        Gizmos.DrawSphere(transform.position, TowerRuntimeStats.Range);
    }

    #endregion

    #region IPlaceable Implementation

    /// <summary>
    /// Called when the tower is being placed.
    /// </summary>
    public void OnPlacing()
    {
        //Debug.Log("Placing tower.");
        UpdateRange();
        Select();
    }

    /// <summary>
    /// Called when the tower is placed.
    /// </summary>
    public void OnPlaced()
    {
        isWorking = true;
        Deselect();
    }

    #endregion

    #region ISelectable Implementation

    public void Select(bool triggerEvent = false)
    {
        rangeGameObject.SetActive(true);
        if (triggerEvent)
            GlobalData.OnTowerSelected?.Invoke(this);
    }

    public void Deselect()
    {
        rangeGameObject.SetActive(false);
        GlobalData.OnTowerSelected?.Invoke(null);
    }

    #endregion


    /// <summary>
    /// Used to place the tower model inside the tower root.
    /// </summary>
    /// <param name="model"></param>
    //public void SetUp(GameObject model, TowerDataUpgradeSO initialStats , Vector3 localPosition = default(Vector3))
    public void SetUp(GameObject model, CardDataSO cardDataSO, int tier = 1, bool applyNextTierStats = false)
    {
        // Stop all coroutines from this tower
        StopAllCoroutines();
        isFiring = false;

        currentCardDataSO = cardDataSO;

        if (applyNextTierStats)
            TowerRuntimeStats  = new(cardDataSO.NextTierData.TowerBaseStats, tier);
        else
            TowerRuntimeStats = new(cardDataSO.TowerBaseStats, tier);

        UpdateRange();
        fireCooldown = TowerRuntimeStats.FireCooldown;

        if (currentModel != null)
        {
            currentModel.transform.SetParent(null);
            DestroyImmediate(currentModel);
        }

        currentModel = model;

        model.transform.SetParent(towerModelContainer);
        model.transform.localPosition = Vector3.zero;
        // TODO: Pass the localPosition in the future

        baseTowerStats = cardDataSO.TowerBaseStats;


        attackHandler.StoreAllFaceTargetInChildren();
        attackHandler.StoreAllProjectileSpawnPoints();
    }

    public bool ApplyUpgrade(CardDataSO cardDataSO)
    {
        Debug.Log("Applying upgrade.");

        var canBeUpgraded = CanUpgradeTower(cardDataSO);

        if (canBeUpgraded)
        {
            TowerRuntimeStats.Tier++;

            GameObject newModel = Instantiate(cardDataSO.TowerInfo.TowerModels[TowerRuntimeStats.Tier - 1]);

            SetUp(newModel, cardDataSO, TowerRuntimeStats.Tier, true);
            Debug.Log("Tower tier upgraded to " + TowerRuntimeStats.Tier);
        }

        return canBeUpgraded;
    }

    public bool CanUpgradeTower(CardDataSO cardDataSO)
    {

        // If the tower's ID is different from the card's ID, return false
        if (TowerInfo.TowerId != cardDataSO.TowerInfo.TowerId)
        {
            Debug.Log("Tower ID is different from the card ID.");
            return false;
        }

        if (TowerRuntimeStats.Tier < MaxTowerTier && TowerRuntimeStats.Tier == cardDataSO.TowerTier)
        {
            return true;
        }

        return false;
    }

    public void ApplyUpgrade(DamageDataUpgrade damageDataUpgrade)
    {
        DamageData damageData = DamageDataList.Find(d => d.DamageType == damageDataUpgrade.DamageType);
        if (damageData == null)
        {
            damageData = new DamageData(damageDataUpgrade.DamageType);
            DamageDataList.Add(damageData);
        }

        damageData.ApplyUpgrade(damageDataUpgrade);
    }
    public void ApplyUpgrade(TowerDataUpgradeSO upgrade, bool levelUp = false)
    {
        TowerRuntimeStats.ApplyUpgrade(upgrade, levelUp);
    }

    public bool ApplyModifier(CardDataSO cardToUse)
    {
        // Check if the tower has empty modifier slots
        int maxSlots = TowerRuntimeStats.Tier;

        Debug.Log("Tower " + gameObject.name + " has " + DamageDataList.Count + " slots of " + maxSlots, gameObject);

        bool canBeApplied = CanApplyModifier();

        if (canBeApplied)
        {
            Debug.Log("Applying modifier.");
            DamageDataList.Add(cardToUse.DamageData);
        }

        return canBeApplied;
    }

    public bool CanApplyModifier()
    {
        // Check if the tower has empty modifier slots
        int maxSlots = TowerRuntimeStats.Tier;

        if (DamageDataList.Count < maxSlots)
        {
            
            return true;
        }
        else
        {
            
            return false;
        }
    }

    protected void PlaceTower(TowerRuntimeStats stats, TowerInfoSO info)
    {
        SetStats(stats);
        SetInfo(info);
        isWorking = true;
    }

    private void SetStats(TowerRuntimeStats stats) => TowerRuntimeStats = stats;
    private void SetInfo(TowerInfoSO _towerInfo) => TowerInfo = _towerInfo;
    
    private void FireRound()
    {
        StartCoroutine(FireRoundCoroutine());
    }

    private IEnumerator FireRoundCoroutine()
    {

        List<GameObject> enemiesToIgnoreIfPossible = new();

        // TODO: By default target different enemies (if in range) for each attack
        isFiring = true;
        for (int i = 0; i < TowerRuntimeStats.FireRate; i++)
        {
            var enemy = AttackEnemy(enemiesToIgnoreIfPossible);
            if (enemy != null && !enemiesToIgnoreIfPossible.Contains(enemy))
                enemiesToIgnoreIfPossible.Add(enemy);

            yield return new WaitForSeconds(1f / TowerRuntimeStats.FireRate);
        }

        // Reset the cooldown after the round is finished
        fireCooldown = TowerRuntimeStats.FireCooldown;
        isFiring = false;
    }

    protected GameObject AttackEnemy(List<GameObject> enemiesToIgnoreIfPossible)
    {
        return attackHandler.Attack(TowerRuntimeStats, DamageDataList, enemiesToIgnoreIfPossible, TowerRuntimeStats.Range + baseTowerStats.Range);
    }

    private void UpdateRange()
    {
        if (baseTowerStats == null) return;

        rangeGameObject.transform.localScale = new Vector3((TowerRuntimeStats.Range + baseTowerStats.Range), 0.01f, (TowerRuntimeStats.Range + baseTowerStats.Range));
    }

    private void OnXPChange(float xp)
    {
        currentXP += xp;
        if (currentXP >= CurrentLevelMaxXP)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        bool canLevelUp = TowerRuntimeStats.Level <= TowerInfo.TowerStatsPerLevel.Count;
        if (canLevelUp)
        {
            currentXP -= CurrentLevelMaxXP;
            TowerDataUpgradeSO newStats = TowerInfo.TowerStatsPerLevel[TowerRuntimeStats.Level - 1].TowerStats;
            ApplyUpgrade(newStats, true);
            Debug.Log("Level up to: " + TowerRuntimeStats.Level);
        }
    }
}
