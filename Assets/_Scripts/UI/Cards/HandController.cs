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

        card.Highlight(true);

        if (card.Prefab != null)
        {
            //objectPlacer.StartPlacing(card.Prefab, isClick);
            objectPlacer.SetUpPlacing(card.Prefab, () => { OnCardUsed(card); }, () => { OnCardNotUsed(card); }, isClick);
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
        card.Highlight(false);
    }

    private void OnCardRemoved(CardMovement cardMovement)
    {
        StartCoroutine(OrderCardsAfterDelay(0.1f));
    }

    private IEnumerator OrderCardsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        handVisualHandler.OrderCardsInWorld();
    }


}
