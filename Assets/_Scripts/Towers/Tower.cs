using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour, IPlaceable, ISelectable
{
    [Header("Initial Data")]
    [SerializeField] private TowerDataUpgradeSO baseTowerStats;

    [Header("Runtime Data")]
    public TowerInfoSO TowerInfo;
    [HideInInspector] public TowerRuntimeStats TowerRuntimeStats;
    public List<DamageData> DamageDataList = new(); // List of damage data for each damage type
    [SerializeField]
    private GameObject rangeGameObject;
    [Tooltip("The container for the tower model. The tower model will be instantiated here.")]
    [SerializeField] private Transform towerModelContainer;
    [SerializeField] private TowerAttackHandler attackHandler;

    private CardDataSO currentCardDataSO;
    private bool isWorking = false;
    private bool isFiring = false;
    private float fireCooldown = 0f;
    private GameObject currentModel;

    [Header("Debug")]
    [SerializeField] private Color debugColor;

    #region Unity Callbacks

    private void Start()
    {
        // Initialize the tower data with the base tower data
        //TowerRuntimeStats = new(baseTowerStats);

        if (TowerInfo == null)
            Debug.LogError("Tower info is not set!");

        Debug.Log("Setting up attack handler.");
        if (attackHandler == null)
            attackHandler = GetComponent<TowerAttackHandler>();
        if (attackHandler == null)
            Debug.LogError("Attack handler is not set on the tower.");
        Debug.Log("Attack handler set up.");
        
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

    public void OnPlacing()
    {
        Debug.Log("Placing tower.");
        UpdateRange();
        Select();
    }

    public void OnPlaced()
    {
        isWorking = true;
        Deselect();
    }

    #endregion

    #region ISelectable Implementation

    public void Select()
    {
        rangeGameObject.SetActive(true);
    }

    public void Deselect()
    {
        rangeGameObject.SetActive(false);
    }

    #endregion

    
    /// <summary>
    /// Used to place the tower model inside the tower root.
    /// </summary>
    /// <param name="model"></param>
    //public void SetUp(GameObject model, TowerDataUpgradeSO initialStats , Vector3 localPosition = default(Vector3))
    public void SetUp(GameObject model, CardDataSO cardDataSO, int tier = 1)
    {
        Debug.Log("SetUp()");
        Debug.Log(cardDataSO.TowerBaseStats.Range);

        // Stop all coroutines from this tower
        StopAllCoroutines();


        currentCardDataSO = cardDataSO;

        TowerRuntimeStats = new(cardDataSO.TowerBaseStats, tier);
        UpdateRange();
        fireCooldown = TowerRuntimeStats.FireCooldown;

        if (currentModel != null)
        {
            Debug.Log("Destroying current model:", currentModel);
            Destroy(currentModel);
            Debug.Log(currentModel);
        }

        currentModel = model;

        Debug.Log("Destroyed children from:", gameObject);
        model.transform.SetParent(towerModelContainer);
        model.transform.localPosition = Vector3.zero;
        // TODO: Pass the localPosition in the future

        baseTowerStats = cardDataSO.TowerBaseStats;


        Debug.Log("End SetUp()");
        attackHandler.StoreAllFaceTargetInChildren();
        attackHandler.StoreAllProjectileSpawnPoints();
    }

    public bool ApplyUpgrade(CardDataSO cardDataSO)
    {
        // If the tower's ID is different from the card's ID, return false
        if (TowerInfo.TowerId != cardDataSO.TowerInfo.TowerId)
        {
            Debug.Log("Tower ID is different from the card ID.");
            return false;
        }

        // If the tower tier is less than the max tower tier and the tower tier is equal to the card's tier, apply the upgrade
        int maxTowerTier = TowerInfo.TowerModels.Count;
        if (TowerRuntimeStats.Tier < maxTowerTier && TowerRuntimeStats.Tier == cardDataSO.TowerTier)
        {
            TowerRuntimeStats.Tier++;

            GameObject newModel = Instantiate(cardDataSO.TowerInfo.TowerModels[TowerRuntimeStats.Tier - 1]);

            SetUp(newModel, cardDataSO, TowerRuntimeStats.Tier);
            Debug.Log("Tower tier upgraded to " + TowerRuntimeStats.Tier);
            return true;
        }
        else
        {
            Debug.LogWarning("Tower tier is already at max level.");
            return false;
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
    public void ApplyUpgrade(TowerDataUpgradeSO upgrade)
    {
        TowerRuntimeStats.ApplyUpgrade(upgrade);
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
        Debug.Log("FireRoundCoroutine()");

        // TODO: By default target different enemies (if in range) for each attack
        isFiring = true;
        for (int i = 0; i < TowerRuntimeStats.FireRate; i++)
        {
            AttackEnemy();

            yield return new WaitForSeconds(1f / TowerRuntimeStats.FireRate);
        }

        // Reset the cooldown after the round is finished
        fireCooldown = TowerRuntimeStats.FireCooldown;
        isFiring = false;
    }

    protected void AttackEnemy()
    {
        attackHandler.Attack(TowerRuntimeStats, DamageDataList);
    }

    private void UpdateRange()
    {
        Debug.Log("Updating range.");
        Debug.Log(baseTowerStats);
        if (baseTowerStats == null) return;

        rangeGameObject.transform.localScale = new Vector3((TowerRuntimeStats.Range + baseTowerStats.Range) * 2, 0.01f, (TowerRuntimeStats.Range + baseTowerStats.Range) * 2);
    }
}
