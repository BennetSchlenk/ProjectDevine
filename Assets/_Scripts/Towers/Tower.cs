using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour, IPlaceable, ISelectable
{
    [Header("Initial Data")]
    [SerializeField] private TowerDataUpgradeSO baseTowerData;

    [Header("Runtime Data")]
    public TowerInfoSO TowerInfo;
    [HideInInspector] public TowerData TowerData;
    public List<DamageData> DamageDataList = new(); // List of damage data for each damage type
    [SerializeField]
    private GameObject rangeGameObject;

    private bool isWorking = false;
    private bool isFiring = false;
    private float fireCooldown = 0f;
    private TowerAttackHandler attackHandler;

    [Header("Debug")]
    [SerializeField] private Color debugColor;

    #region Unity Callbacks

    private void Start()
    {
        // Initialize the tower data with the base tower data
        TowerData = new(baseTowerData);

        if (TowerInfo == null)
            Debug.LogError("Tower info is not set!");

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
        if (TowerData == null) return;

        Gizmos.color = debugColor;
        Gizmos.DrawSphere(transform.position, TowerData.Range);
    }

    #endregion

    #region IPlaceable Implementation

    public void OnPlacing()
    {
        UpdateRange();
        Select();
    }

    public void OnPlaced()
    {
        Debug.Log("Tower placed!", gameObject);
        isWorking = true;
        Debug.Log("Tower is working: " + isWorking, gameObject);
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
        TowerData.ApplyUpgrade(upgrade);
    }

    protected void PlaceTower(TowerData stats, TowerInfoSO info)
    {
        SetStats(stats);
        SetInfo(info);
        isWorking = true;
    }

    private void SetStats(TowerData stats) => TowerData = stats;
    private void SetInfo(TowerInfoSO _towerInfo) => TowerInfo = _towerInfo;
    
    private void FireRound()
    {
        StartCoroutine(FireRoundCoroutine());
    }

    private IEnumerator FireRoundCoroutine()
    {
        isFiring = true;
        for (int i = 0; i < TowerData.FireRate; i++)
        {
            AttackEnemy();

            yield return new WaitForSeconds(1f / TowerData.FireRate);
        }

        // Reset the cooldown after the round is finished
        fireCooldown = TowerData.FireCooldown;
        isFiring = false;
    }

    protected void AttackEnemy()
    {
        attackHandler.Attack(TowerData, DamageDataList);
    }

    private void UpdateRange()
    {
        rangeGameObject.transform.localScale = new Vector3((TowerData.Range + baseTowerData.Range) * 2, 0.01f, (TowerData.Range + baseTowerData.Range) * 2);
    }
}
