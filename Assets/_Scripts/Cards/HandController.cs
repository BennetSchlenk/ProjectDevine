using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(HandVisualHandler))]
public class HandController : MonoBehaviour
{
    public event Action OnHandUpdate = delegate { };

    [SerializeField] private BezierCurve bezierCurve;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private ObjectPlacer objectPlacer;

    public Transform CardsContainer => cardsContainer;

    private List<BezierChildMovement> cards = new();
    private HandVisualHandler handVisualHandler;
    private Card lastCardSelected;

    #region Unity Callbacks
        
    private void Awake()
    {
        handVisualHandler = GetComponent<HandVisualHandler>();
        handVisualHandler.OnCardDraggedAction += UseCardFromDrag;
        handVisualHandler.OnCardClickedAction += UseCardFromClick;
        handVisualHandler.OnCardRemoveAction += OnCardRemoved;
    }

    private void OnDestroy()
    {
        handVisualHandler.OnCardDraggedAction -= UseCardFromDrag;
        handVisualHandler.OnCardClickedAction -= UseCardFromClick;
        handVisualHandler.OnCardRemoveAction -= OnCardRemoved;
    }

    #endregion

    /// <summary>
    /// Use a card when it is clicked.
    /// </summary>
    /// <param name="cardMovement"></param>
    private void UseCardFromClick(CardMovement cardMovement) => UseCard(cardMovement, true);
    /// <summary>
    /// Use a card when it is dragged.
    /// </summary>
    /// <param name="cardMovement"></param>
    private void UseCardFromDrag(CardMovement cardMovement) => UseCard(cardMovement, false);

    private void UseCard(CardMovement cardMovement, bool isClick = false)
    {
        Card card = cardMovement.GetComponent<Card>();
        if (card == null) return;

        SelectCard(card, true);

        if (card.TowerPrefab != null)
            objectPlacer.UseCard(card.CardData, () => { OnCardUsed(card); }, () => { OnCardNotUsed(card); }, isClick);
    }

    private void OnCardUsed(Card card)
    {
        Destroy(card.gameObject);
        handVisualHandler.OrderCards();
    }

    private void OnCardNotUsed(Card card) => SelectCard(card, false);

    /// <summary>
    /// Orders the cards after a card has been removed.
    /// </summary>
    /// <param name="cardMovement"></param>
    private void OnCardRemoved(CardMovement cardMovement)
    {
        if (!gameObject.activeInHierarchy) return; // If the game object is not active, return to avoid Coroutine errors.

        StartCoroutine(OrderCardsAfterDelay(0f));
    }

    private IEnumerator OrderCardsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        handVisualHandler.OrderCards();
    }

    private void SelectCard(Card card, bool highlight)
    {
        if (lastCardSelected != null)
            lastCardSelected.Highlight(false);

        lastCardSelected = card;

        if (highlight)
            card.Highlight(true);
    }

}
