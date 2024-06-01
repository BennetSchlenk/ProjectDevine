using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterPack : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI packNameText;
    [SerializeField] private TMPro.TextMeshProUGUI packCostText;
    [SerializeField] private Image packIcon;

    [SerializeField] private BoosterPackSO boosterPackSO;

    private PlayerDataManager playerDataManager;

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        playerDataManager = ServiceLocator.Instance.GetService<PlayerDataManager>();

        UpdateInfo();
    }

    #endregion

    public void Buy()
    {

       if (GlobalData.HandController.CanAddCards(boosterPackSO.totalCards) && playerDataManager.RemoveEssence(boosterPackSO.PackCost))
        {
            // Open the pack
            List<CardDataSO> cards = boosterPackSO.OpenPack();
            // Add the cards to the player's collection
            GlobalData.HandController.GiveCards(cards);
        }
    }

    public void SetBoosterPack(BoosterPackSO boosterPack)
    {
        boosterPackSO = boosterPack;
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        packNameText.text = boosterPackSO.PackName;
        packCostText.text = boosterPackSO.PackCost.ToString();
        packIcon.sprite = boosterPackSO.PackIcon;
    }

}
