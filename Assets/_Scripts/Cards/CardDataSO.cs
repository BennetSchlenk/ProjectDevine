using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CardData", menuName = "Project Divine/Cards/CardData")]
public class CardDataSO : ScriptableObject
{
    public int Id;
    public string Name;
    public string Description;
    public Sprite CardFrame;
    public Sprite Icon;
    public CardType Type;
    public int Cost;

    [Header("Tower Settings")]

    [Tooltip("The tier of the tower card: 1-Basic, 2-Advanced, 3-Elite")]
    public int TowerTier = 1;
    [Tooltip("The tower info for the card")]
    public TowerInfoSO TowerInfo;
    public TowerDataUpgradeSO TowerBaseStats;
    public TowerAttackType TowerAttackType;
    public float TowerAttackArea;
    
    public CardDataSO NextTierData;
    public GameObject TowerPrefab => TowerInfo.TowerModels[TowerTier - 1];

    [Header("DamageData Settings")]
    [Tooltip("The damage data for modifier cards. If it is a tower, this will be the default damage.")]
    public DamageData DamageData;
}

public enum CardType
{
    Tower,
    Modifier,
    Ability
}

public enum TowerAttackType
{
    Target,
    Area,
    TowerArea,
}
