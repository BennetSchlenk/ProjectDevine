using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Holds the card data and highlight state.
/// </summary>
public class Card : MonoBehaviour
{
    [SerializeField] private CardDataSO cardData;
    [SerializeField] private GameObject highlightFrame;
    [SerializeField] private TMPro.TextMeshProUGUI cardName;
    [SerializeField] private TMPro.TextMeshProUGUI cardDescription;
    [SerializeField] private Image cardImage;

    public CardDataSO CardData => cardData;
    public GameObject TowerPrefab => cardData.TowerPrefab;

    #region Unity Callbacks

    private void Start()
    {
        if (CardData != null)
            RefreshInfo();
    }

    #endregion

    public void SetUp(CardDataSO cardData)
    {
        this.cardData = cardData;
        RefreshInfo();
    }

    /// <summary>
    /// Refresh the card information with the current card data.
    /// </summary>
    public void RefreshInfo()
    {
        cardName.text = cardData.Name;
        cardDescription.text = cardData.Description;
        cardImage.sprite = cardData.Icon;
    }

    /// <summary>
    /// Highlight the card by toggling the highlight frame GameObject.
    /// </summary>
    /// <param name="toggle">Enable the highlight frame?</param>
    public void Highlight(bool toggle) => highlightFrame.SetActive(toggle);
}
