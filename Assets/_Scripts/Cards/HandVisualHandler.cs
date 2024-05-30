using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// HandVisualHandler is responsible for the visual management of cards in a player's hand in a card game.
/// This class handles the animation and positioning of cards in the player's hand, as well as user interaction with the cards.
/// </summary>
[RequireComponent(typeof(Hand))]
public class HandVisualHandler : MonoBehaviour
{
    // Card Events
    public event Action<CardMovement> OnCardClickedAction = delegate { };
    public event Action<CardMovement> OnCardDraggedAction = delegate { };
    public event Action<CardMovement> OnCardDroppedAction = delegate { };
    public event Action<CardMovement> OnCardRemoveAction = delegate { };


    [Header("Transition Settings (seconds)")]

    [Tooltip("Time that it takes to reorder cards in hand once another one is added or removed.")]
    [SerializeField] private float reorderTime;

    [Tooltip("Time that it takes for a card to be placed correctly once added to the hand.")]
    [SerializeField] private float initialPlaceTime;

    [Tooltip("Time that it takes for a card to move to discarded deck.")]
    [SerializeField] private float discardTime;

    [Tooltip("Transform to take the Y position of cards that are hovered.")]
    [SerializeField] private Transform hoverPositionTransform;

    // Components
    private HandController handController;
    private BezierCurve _bezierCurve;
    private Transform cardsContainer;

    // Variables
    private CardMovement draggingCard;

    #region Unity Callbacks

    private void Awake()
    {
        handController = GetComponent<HandController>();
        _bezierCurve = GetComponentInChildren<BezierCurve>();
    }

    void Start()
    {
        cardsContainer = handController.CardsContainer;
        OrderCards();
        handController.OnHandUpdate += OnHandUpdate;
    }

    private void OnDestroy()
    {
        handController.OnHandUpdate -= OnHandUpdate;

        OnCardDraggedAction = null;
        OnCardDroppedAction = null;
        OnCardRemoveAction = null;
        OnCardClickedAction = null;
    }

    #endregion

    /// <summary>
    /// Order cards in hand according to the Bezier curve.
    /// TODO: Use AnimationCurve to define the distance between cards.
    /// </summary>
    [ContextMenu("Order Cards")]
    public void OrderCards()
    {
        List<Transform> children = GetWorldCards();

        if (children == null || children.Count == 0) return;

        // If children.Count is 1, set the card in the middle of the curve
        if (children.Count == 1)
        {
            children[0].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.5f), _bezierCurve.ControlPoints[3].rotation, initialPlaceTime);
        }
        else if (children.Count == 2)
        {
            children[0].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.25f), _bezierCurve.GetCardOrientation(0.25f), initialPlaceTime);
            children[1].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.75f), _bezierCurve.GetCardOrientation(0.75f), initialPlaceTime);
        }
        else if (children.Count == 3)
        {
            children[0].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.2f), _bezierCurve.GetCardOrientation(0.2f), initialPlaceTime);
            children[1].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.5f), _bezierCurve.GetCardOrientation(0.5f), initialPlaceTime);
            children[2].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.8f), _bezierCurve.GetCardOrientation(0.8f), initialPlaceTime);
        }
        else
        {
            // Position the cards in the Bezier curve with the same distance between them
            for (int i = 0; i < children.Count; i++)
            {
                Vector3 cardFinalPosition = _bezierCurve.GetBezierPoint((float)i / (children.Count - 1));
                Quaternion cardFinalRotation = _bezierCurve.GetCardOrientation((float)i / (children.Count - 1));

                CardMovement cardMovement = children[i].GetComponent<CardMovement>();

                cardMovement.MoveToPosition(cardFinalPosition, cardFinalRotation, reorderTime);
            }
        }

        SetCardProperties(children);
    }

    private void SetCardProperties(List<Transform> children)
    {
        for (int i = 0; i < children.Count; i++)
        {
            CardMovement cardMovement = children[i].GetComponent<CardMovement>();
            cardMovement.SetHoveredPositionTransform(hoverPositionTransform);
            children[i].SetSiblingIndex(i);
            cardMovement.OnCardDragged += OnCardDragged;
            cardMovement.OnCardDropped += OnCardDropped;
            cardMovement.OnCardRemove += OnCardRemove;
            cardMovement.OnCardClicked += OnCardClicked;
        }
    }

    /// <summary>
    /// Retrieve the cards in the hand based on the children of the cards container.
    /// </summary>
    /// <returns>List of cards Transforms</returns>
    private List<Transform> GetWorldCards()
    {
        List<Transform> children = new List<Transform>();

        foreach (Transform t in cardsContainer)
            children.Add(t);

        return children;
    }

    private void OnHandUpdate() => OrderCards();
    private void OnCardClicked(CardMovement cardMovement) => OnCardClickedAction(cardMovement);

    /// <summary>
    /// Unsubscribe from the card events and call the OnCardRemoveAction.
    /// </summary>
    /// <param name="cardMovement"></param>
    private void OnCardRemove(CardMovement cardMovement)
    {
        cardMovement.OnCardDragged -= OnCardDragged;
        cardMovement.OnCardDropped -= OnCardDropped;
        cardMovement.OnCardRemove -= OnCardRemove;
        cardMovement.OnCardClicked -= OnCardClicked;
        OnCardRemoveAction(cardMovement);
    }

    #region Drag & Drop

    private void OnCardDragged(CardMovement cardMovement)
    {
        draggingCard = cardMovement;
        OnCardDraggedAction(cardMovement);
    }

    private void OnCardDropped(CardMovement cardMovement) => OnCardDroppedAction(cardMovement);

    #endregion

}
