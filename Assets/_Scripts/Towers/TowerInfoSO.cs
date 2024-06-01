using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TowerInfoSO is a ScriptableObject that holds static information about a tower: Name, Description, Sprite, and Tier Models.
/// </summary>
[CreateAssetMenu(fileName = "New TowerInfo", menuName = "Project Divine/Towers/Tower Info")]
public class TowerInfoSO : ScriptableObject
{
    [SerializeField] private int towerId;
    public int TowerId => towerId;

    [SerializeField] private string towerName;
    public string TowerName => towerName;

    [SerializeField] private string towerDescription;
    public string TowerDescription => towerDescription;

    [SerializeField] private Sprite towerSprite;
    public Sprite TowerSprite => towerSprite;

    [Tooltip("The tower models (per tier) that will be instantiated when the tower is placed.")]
    [SerializeField] private List<GameObject> towerModels;
    public List<GameObject> TowerModels => towerModels;

    [Tooltip("The tower stats per level.")]
    [SerializeField] private List<TowerStatsPerLevel> towerStatsPerLevel;
    public List<TowerStatsPerLevel> TowerStatsPerLevel => towerStatsPerLevel;
}

[System.Serializable]
public class TowerStatsPerLevel
{
    public int XP;
    public TowerDataUpgradeSO TowerStats;
}