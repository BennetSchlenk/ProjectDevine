using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private CardDataSO cardData;
    public CardDataSO CardData => cardData;

    public GameObject TowerPrefab => cardData.TowerPrefab;

    [SerializeField] private GameObject highlightFrame;
    [SerializeField] private TMPro.TextMeshProUGUI cardName;
    [SerializeField] private TMPro.TextMeshProUGUI cardDescription;
    [SerializeField] private Image cardImage;

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        if (CardData != null)
            RefreshInfo();
    }

    #endregion

    public void RefreshInfo()
    {
        cardName.text = cardData.Name;
        cardDescription.text = cardData.Description;
        cardImage.sprite = cardData.Icon;
    }
    public void Highlight(bool toggle)
    {
        highlightFrame.SetActive(toggle);
    }
}
