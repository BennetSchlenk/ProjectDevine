using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterPack : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI packNameText;
    [SerializeField] private TMPro.TextMeshProUGUI packCostText;
    [SerializeField] private Image packIcon;
    [SerializeField] private Button buyButton;
    [SerializeField] private float buyCooldown = 1f;
    [SerializeField] private Button[] otherBoosterPackbuttons;

    [SerializeField] private BoosterPackSO boosterPackSO;

    private PlayerDataManager playerDataManager;
    private AudioManager audioManager;

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        playerDataManager = ServiceLocator.Instance.GetService<PlayerDataManager>();
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();

        buyButton.onClick.AddListener(Buy);

        UpdateInfo();
    }

    private void OnDestroy()
    {
        buyButton.onClick.RemoveListener(Buy);
    }

    #endregion

    public void Buy()
    {

        if (GlobalData.HandController.CanAddCards(boosterPackSO.totalCards) && playerDataManager.RemoveEssence(boosterPackSO.PackCost))
        {

            StartCoroutine(BuyCooldownEnumerator());
        }
        else
            StartCoroutine(CannotBuyCooldownEnumerator());
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

    private IEnumerator BuyCooldownEnumerator()
    {
        ToggleButtons(false);
        audioManager.PlaySFXOneShotAtPosition("buy", transform.position);
        yield return new WaitForSeconds(0.5f);
        // Open the pack
        List<CardDataSO> cards = boosterPackSO.OpenPack();
        // Add the cards to the player's collection
        GlobalData.HandController.GiveCards(cards);
        yield return new WaitForSeconds(buyCooldown);
        ToggleButtons(true);
    }

    private IEnumerator CannotBuyCooldownEnumerator()
    {
        ToggleButtons(false);
        audioManager.PlaySFXOneShotAtPosition("cardPlaceWrong", transform.position);
        yield return new WaitForSeconds(1f);
        ToggleButtons(true);
    }

    private void ToggleButtons(bool toggle)
    {
        buyButton.interactable = toggle;
        foreach (Button button in otherBoosterPackbuttons)
        {
            button.interactable = toggle;
        }
    }
}
