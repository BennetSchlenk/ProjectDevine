using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(HandVisualHandler))]
public class HandController : MonoBehaviour
{
    public event Action OnHandUpdate = delegate { };

    [SerializeField] private BezierCurve bezierCurve;
    [SerializeField] private BasePool cardPool;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private ObjectPlacer objectPlacer;
    [SerializeField] private Transform cardSpawnPoint;

    public Transform CardsContainer => cardsContainer;

    private List<Transform> cards = new();
    public List<Transform> Cards => cards;
    private HandVisualHandler handVisualHandler;
    private Card lastCardSelected;
    private int maxCards = 10;
    private PlayerDataManager playerDataManager;
    private AudioManager audioManager;

    [Header("Debug")]
    [SerializeField] private CardDataSO cardDataDebug;

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


    // TODO: Remove Start and GiveCardsPeriodically
    private void Start()
    {
        playerDataManager = ServiceLocator.Instance.GetService<PlayerDataManager>();
        audioManager = ServiceLocator.Instance.GetService<AudioManager>();

        GlobalData.HandController = this;

        //StartCoroutine(GiveCardsPeriodically(2f));
    }

    private IEnumerator GiveCardsPeriodically(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            GiveCardDebug();
        }
    }

    #endregion

    [ContextMenu("Give Card Debug")]
    public void GiveCardDebug() => GiveCard(cardDataDebug);

    public void GiveCards(List<CardDataSO> cards)
    {
        StartCoroutine(GiveCards(cards, 0.25f));        
    }

    private IEnumerator GiveCards(List<CardDataSO> cards, float delay)
    {
        foreach (CardDataSO card in cards)
        {
            GiveCard(card);
            yield return new WaitForSeconds(delay);
        }
    }

    public void GiveCard(CardDataSO cardData)
    {
        if (cardData == null || cards.Count >= maxCards) return;

        // Instantiate and set up the card
        GameObject card = cardPool.pool.Get().gameObject;
        card.transform.SetParent(cardsContainer);
        card.transform.localPosition = cardSpawnPoint.localPosition;
        card.GetComponent<Card>().SetUp(cardData);

        OnCardAdded(card);
        audioManager.PlaySFXOneShotAtPosition("cardDeal", transform.position);
    }

    private void RefreshHand()
    {
        cards = handVisualHandler.GetWorldCards();
        OnHandUpdate?.Invoke();
    }

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


        if (card.CanBeUsed)
        {
            SelectCard(card, true);
            objectPlacer.UseCard(card.CardData, () => { OnCardUsed(card); }, () => { OnCardNotUsed(card); }, isClick);
            GlobalData.OnCardDragged?.Invoke(card.CardData);
        } else
        {
            Debug.Log("<color=red>Card cannot be used. Not enough essence!</color>");
        }
    }

    private void OnCardAdded(GameObject cardGO)
    {
        RefreshHand();
    }

    private void OnCardUsed(Card card)
    {
        audioManager.PlaySFXOneShotAtPosition("cardPlace", transform.position);

        DestroyImmediate(card.gameObject);
        handVisualHandler.OrderCards();
        GlobalData.OnCardDragged?.Invoke(null);
        playerDataManager.RemoveEssence(card.CardData.Cost);
        RefreshHand();
    }

    private void OnCardNotUsed(Card card)
    {
        SelectCard(card, false);
        GlobalData.OnCardDragged?.Invoke(null);
    }

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

        audioManager.PlaySFXOneShotAtPosition("cardClick", transform.position);
    }

    public bool CanAddCards(int totalCards)
    {
        return cards.Count + totalCards <= maxCards;
    }

    
}
