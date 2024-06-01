using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerInfoPanel : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI towerName;
    [SerializeField] private TMPro.TextMeshProUGUI towerDescription;
    [SerializeField] private TMPro.TextMeshProUGUI towerCost;
    [SerializeField] private Transform modifiersContainer;
    [SerializeField] private Transform statsContainer;
    [SerializeField] private Transform lockedPlaceholdersContainer;
    [SerializeField] private TMPro.TextMeshProUGUI currentLevelText;
    [SerializeField] private TMPro.TextMeshProUGUI nextLevelText;
    [SerializeField] private Image levelProgressBar;

    [SerializeField] private GameObject modifierPrefab;
    [SerializeField] private GameObject statPrefab;
    [SerializeField] private GameObject lockedPlaceholderPrefab;
    [SerializeField] private Sprite lockedIcon;
    [SerializeField] private Sprite unlockedIcon;
    [SerializeField] private Sprite rangeIcon;
    [SerializeField] private Sprite projectileSpeedIcon;
    [SerializeField] private Sprite fireRateIcon;
    [SerializeField] private Sprite fireDurationIcon;
    [SerializeField] private Sprite fireCooldownIcon;

    private Tower tower;

    public void SetTower(Tower tower)
    {
        towerName.text = tower.TowerInfo.TowerName;
        towerDescription.text = tower.TowerInfo.TowerDescription;
        towerCost.text = tower.CurrentCardDataSO.Cost.ToString();

        // Destroy all Transforms inside the modifiers container
        foreach (Transform child in modifiersContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var modifier in tower.DamageDataList)
        {
            GameObject modifierGO = Instantiate(modifierPrefab, modifiersContainer);
            modifierGO.GetComponent<ModifierUI>().SetUp(modifier);
        }

        // Destroy placeholders in the locked placeholders container
        foreach (Transform child in lockedPlaceholdersContainer)
        {
            Destroy(child.gameObject);
        }

        // Create as much placeholders as the tower has modifiers
        for (int i = 0; i < tower.MaxTowerTier; i++)
        {
            GameObject placeholderGO = Instantiate(lockedPlaceholderPrefab, lockedPlaceholdersContainer);
            placeholderGO.GetComponent<Image>().sprite = i < tower.MaxTowerModifiers ? unlockedIcon : lockedIcon;
        }

        // Clear the stats container
        foreach (Transform child in statsContainer)
        {
            Destroy(child.gameObject);
        }

        // Show the tower stats in the stats container
        GameObject rangeStatGO = Instantiate(statPrefab, statsContainer);
        rangeStatGO.GetComponent<ModifierAttributeUI>().SetUp(tower.TowerRuntimeStats.Range, rangeIcon);
        
        GameObject projectileSpeedStatGO = Instantiate(statPrefab, statsContainer);
        projectileSpeedStatGO.GetComponent<ModifierAttributeUI>().SetUp(tower.TowerRuntimeStats.ProjectileSpeed, projectileSpeedIcon);

        GameObject fireRateStatGO = Instantiate(statPrefab, statsContainer);
        fireRateStatGO.GetComponent<ModifierAttributeUI>().SetUp(tower.TowerRuntimeStats.FireRate, fireRateIcon);

        GameObject fireDurationStatGO = Instantiate(statPrefab, statsContainer);
        fireDurationStatGO.GetComponent<ModifierAttributeUI>().SetUp(tower.TowerRuntimeStats.FireDuration, fireDurationIcon);

        GameObject fireCooldownStatGO = Instantiate(statPrefab, statsContainer);
        fireCooldownStatGO.GetComponent<ModifierAttributeUI>().SetUp(tower.TowerRuntimeStats.FireCooldown, fireCooldownIcon);

        // Show level stats
        currentLevelText.text = tower.TowerRuntimeStats.Level.ToString();
        int nextLevel = tower.TowerRuntimeStats.Level + 1;
        string finalStr = nextLevel.ToString();
        if (nextLevel >= tower.TowerInfo.TowerStatsPerLevel.Count+1)
        {
            nextLevel = tower.TowerInfo.TowerStatsPerLevel.Count;
            finalStr = "Max";
        }
        nextLevelText.text = finalStr;
        levelProgressBar.fillAmount = (float)tower.CurrentXP / tower.CurrentLevelMaxXP;


    }
}
