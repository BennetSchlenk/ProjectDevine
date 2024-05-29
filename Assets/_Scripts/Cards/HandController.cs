using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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

    private void Start()
    {
        //Refresh();
    }

    private void OnDestroy()
    {
        handVisualHandler.OnCardDraggedAction -= UseCardFromDrag;
        handVisualHandler.OnCardClickedAction -= UseCardFromClick;
        handVisualHandler.OnCardRemoveAction -= OnCardRemoved;
    }

    #endregion

    // TODO: Is this used?
    [ContextMenu("Refresh")]
    public void Refresh()
    {
        // Destroy cards
        //foreach (var card in cards)
        //    Destroy(card.gameObject);

        cards.Clear();

        foreach (Transform transf in cardsContainer)
        {
            BezierChildMovement bcm = transf.GetComponent<BezierChildMovement>();
            bcm.SetBezierCurve(bezierCurve);
            if (bcm != null)
                cards.Add(bcm);
        }

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].T = (float)i / (cards.Count - 1);
        }


    }

    private void UseCardFromClick(CardMovement cardMovement) => UseCard(cardMovement, true);
    private void UseCardFromDrag(CardMovement cardMovement) => UseCard(cardMovement, false);

    private void UseCard(CardMovement cardMovement, bool isClick = false)
    {
        Card card = cardMovement.GetComponent<Card>();
        if (card == null) return;

        SelectCard(card, true);

        if (card.TowerPrefab != null)
        {
            //objectPlacer.StartPlacing(card.Prefab, isClick);
            objectPlacer.UseCard(card.CardData, () => { OnCardUsed(card); }, () => { OnCardNotUsed(card); }, isClick);
        }


        //card.Use();
        //Refresh();
    }

    private void OnCardUsed(Card card)
    {
        //card.Highlight(false);
        Debug.Log("-------------Card used");
        Destroy(card.gameObject);
        handVisualHandler.OrderCardsInWorld();
    }

    private void OnCardNotUsed(Card card)
    {
        Debug.Log("-------------Card not used");
        SelectCard(card, false);
    }

    private void OnCardRemoved(CardMovement cardMovement)
    {
        if (!gameObject.activeInHierarchy) return; // Remove the editor error when leaving play mode

        StartCoroutine(OrderCardsAfterDelay(0f));
    }

    private IEnumerator OrderCardsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        handVisualHandler.OrderCardsInWorld();
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
