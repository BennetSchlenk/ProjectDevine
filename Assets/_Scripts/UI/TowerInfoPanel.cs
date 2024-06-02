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
    [SerializeField] private Button attackTargetClosestButton;
    [SerializeField] private Button attackTargetFarthestButton;
    [SerializeField] private Button attackTargetWeakestButton;
    [SerializeField] private Button attackTargetStrongestButton;

    [SerializeField] private Sprite attackTypeSelectedImage;
    [SerializeField] private Sprite attackTypeUnselectedImage;
    [SerializeField] private GameObject modifierPrefab;
    [SerializeField] private GameObject statPrefab;
    [SerializeField] private GameObject lockedPlaceholderPrefab;
    [SerializeField] private Sprite lockedIcon;
    [SerializeField] private Sprite unlockedIcon;
    [SerializeField] private Sprite lockedFrame;
    [SerializeField] private Sprite unlockedFrame;
    [SerializeField] private Sprite rangeIcon;
    [SerializeField] private Sprite projectileSpeedIcon;
    [SerializeField] private Sprite fireRateIcon;
    [SerializeField] private Sprite fireDurationIcon;
    [SerializeField] private Sprite fireCooldownIcon;

    private Tower tower;
    public float LastTimeButtonPressed;

    private void Start()
    {
        attackTargetClosestButton.onClick.AddListener(SetClosest);
        attackTargetFarthestButton.onClick.AddListener(SetFarthest);
        attackTargetWeakestButton.onClick.AddListener(SetWeakest);
        attackTargetStrongestButton.onClick.AddListener(SetStrongest);
    }

    private void OnDestroy()
    {
        attackTargetClosestButton.onClick.RemoveListener(SetClosest);
        attackTargetFarthestButton.onClick.RemoveListener(SetFarthest);
        attackTargetWeakestButton.onClick.RemoveListener(SetWeakest);
        attackTargetStrongestButton.onClick.RemoveListener(SetStrongest);
    }

    public void SetTower(Tower _tower)
    {

        if (_tower == null)
        {
            Debug.Log(Time.time - LastTimeButtonPressed);

            if (Time.time - LastTimeButtonPressed <= 0.5f)
            {
                Refresh();
            }

            gameObject.SetActive(false);
            return;
        }

        tower = _tower;

        towerName.text = _tower.TowerInfo.TowerName;
        towerDescription.text = _tower.TowerInfo.TowerDescription;
        towerCost.text = _tower.CurrentCardDataSO.Cost.ToString();

        // Destroy all Transforms inside the modifiers container
        foreach (Transform child in modifiersContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var modifier in _tower.DamageDataList)
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
        for (int i = 0; i < _tower.MaxTowerTier; i++)
        {
            GameObject placeholderGO = Instantiate(lockedPlaceholderPrefab, lockedPlaceholdersContainer);
            LockedStatusPanel lockedStatusPanel = placeholderGO.GetComponent<LockedStatusPanel>();
            var icon = i < _tower.MaxTowerModifiers ? unlockedIcon : lockedIcon;
            var frame = i < _tower.MaxTowerModifiers ? unlockedFrame : lockedFrame;
            lockedStatusPanel.SetStatus(frame, icon);
        }

        // Clear the stats container
        foreach (Transform child in statsContainer)
        {
            Destroy(child.gameObject);
        }

        UpdateButtonImages();

        // Show the tower stats in the stats container
        GameObject rangeStatGO = Instantiate(statPrefab, statsContainer);
        rangeStatGO.GetComponent<ModifierAttributeUI>().SetUp(_tower.TowerRuntimeStats.Range, rangeIcon);
        
        GameObject projectileSpeedStatGO = Instantiate(statPrefab, statsContainer);
        projectileSpeedStatGO.GetComponent<ModifierAttributeUI>().SetUp(_tower.TowerRuntimeStats.ProjectileSpeed, projectileSpeedIcon);

        GameObject fireRateStatGO = Instantiate(statPrefab, statsContainer);
        fireRateStatGO.GetComponent<ModifierAttributeUI>().SetUp(_tower.TowerRuntimeStats.FireRate, fireRateIcon);

        GameObject fireDurationStatGO = Instantiate(statPrefab, statsContainer);
        fireDurationStatGO.GetComponent<ModifierAttributeUI>().SetUp(_tower.TowerRuntimeStats.FireDuration, fireDurationIcon);

        GameObject fireCooldownStatGO = Instantiate(statPrefab, statsContainer);
        fireCooldownStatGO.GetComponent<ModifierAttributeUI>().SetUp(_tower.TowerRuntimeStats.FireCooldown, fireCooldownIcon);

        // Show level stats
        currentLevelText.text = _tower.TowerRuntimeStats.Level.ToString();
        int nextLevel = _tower.TowerRuntimeStats.Level + 1;
        string finalStr = nextLevel.ToString();
        if (nextLevel > _tower.MaxLevel)
        {
            nextLevel = _tower.MaxLevel;
            finalStr = "Max";
        }
        nextLevelText.text = finalStr;
        levelProgressBar.fillAmount = (float)_tower.CurrentXP / _tower.CurrentLevelMaxXP;


    }

    private void SetClosest()
    {
        ChangeAttackType(0);
    }

    private void SetFarthest()
    {
        ChangeAttackType(1);
    }

    private void SetWeakest()
    {
        ChangeAttackType(3);
    }

    private void SetStrongest()
    {
        ChangeAttackType(2);
    }

    public void ChangeAttackType(int type)
    {
        tower.AttackHandler.AttackTargetType = (AttackTargetType)(TowerAttackType)type;
        LastTimeButtonPressed = Time.time;

        Refresh();
    }

    public void Refresh()
    {
        SetTower(tower);
    }

    private void UpdateButtonImages()
    {
        // Reset all buttons to unselected
        attackTargetClosestButton.image.sprite = attackTypeUnselectedImage;
        attackTargetFarthestButton.image.sprite = attackTypeUnselectedImage;
        attackTargetWeakestButton.image.sprite = attackTypeUnselectedImage;
        attackTargetStrongestButton.image.sprite = attackTypeUnselectedImage;
        // Set the selected button to selected
        switch (tower.AttackHandler.AttackTargetType)
        {
            case AttackTargetType.Closest:
                attackTargetClosestButton.image.sprite = attackTypeSelectedImage;
                break;
            case AttackTargetType.Farthest:
                attackTargetFarthestButton.image.sprite = attackTypeSelectedImage;
                break;
            case AttackTargetType.Weakest:
                attackTargetWeakestButton.image.sprite = attackTypeSelectedImage;
                break;
            case AttackTargetType.Strongest:
                attackTargetStrongestButton.image.sprite = attackTypeSelectedImage;
                break;
        }
    }
}
