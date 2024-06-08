using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// HandVisualHandler is responsible for the visual management of cards in a player's hand in a card game.
/// This class handles the animation and positioning of cards in the player's hand, as well as user interaction with the cards.
/// </summary>
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

    [SerializeField] private AnimationCurve curve;

    private float4 maxValX, minValX;
    private float4 maxValY, minValY;

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
        minValX = new float4(-75, -25, 25, 75);
        maxValX = new float4(-400, -200, 200, 400);
        minValY = new float4(-36, -31, -31, -36);
        maxValY = new float4(-36, 15, 15, -36);

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

    private void OnRectTransformDimensionsChange() => OrderCards();

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
            var pos1 = _bezierCurve.ControlPoints[0].localPosition;
            pos1.x = minValX.x;
            pos1.y = minValY.x;
            _bezierCurve.ControlPoints[0].localPosition = pos1;

            var pos2 = _bezierCurve.ControlPoints[1].localPosition;
            pos2.x = minValX.y;
            pos2.y = minValY.y;
            _bezierCurve.ControlPoints[1].localPosition = pos2;

            var pos3 = _bezierCurve.ControlPoints[2].localPosition;
            pos3.x = minValX.z;
            pos3.y = minValY.z;
            _bezierCurve.ControlPoints[2].localPosition = pos3;

            var pos4 = _bezierCurve.ControlPoints[3].localPosition;
            pos4.x = minValX.w;
            pos4.y = minValY.w;
            _bezierCurve.ControlPoints[3].localPosition = pos4;

            children[0].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.5f),
                _bezierCurve.ControlPoints[3].rotation, initialPlaceTime);
        }
        else
        {
            // Position the cards in the Bezier curve with the same distance between them

            var val = curve.Evaluate(children.Count);
            var newVal1 = math.remap(0, 1, maxValX.x, minValX.x, val);
            var newVal2 = math.remap(0, 1, maxValX.y, minValX.y, val);
            var newVal3 = math.remap(0, 1, maxValX.z, minValX.z, val);
            var newVal4 = math.remap(0, 1, maxValX.w, minValX.w, val);

            var newValY1 = math.remap(0, 1, maxValY.x, minValY.x, val);
            var newValY2 = math.remap(0, 1, maxValY.y, minValY.y, val);
            var newValY3 = math.remap(0, 1, maxValY.z, minValY.z, val);
            var newValY4 = math.remap(0, 1, maxValY.w, minValY.w, val);


            var pos1 = _bezierCurve.ControlPoints[0].localPosition;
            pos1.x = (int)newVal1;
            pos1.y = (int)newValY1;
            _bezierCurve.ControlPoints[0].localPosition = pos1;



            var pos2 = _bezierCurve.ControlPoints[1].localPosition;
            pos2.x = (int)newVal2;
            pos2.y = (int)newValY2;
            _bezierCurve.ControlPoints[1].localPosition = pos2;

            var pos3 = _bezierCurve.ControlPoints[2].localPosition;
            pos3.x = (int)newVal3;
            pos3.y = (int)newValY3;
            _bezierCurve.ControlPoints[2].localPosition = pos3;

            var pos4 = _bezierCurve.ControlPoints[3].localPosition;
            pos4.x = (int)newVal4;
            pos4.y = (int)newValY4;
            _bezierCurve.ControlPoints[3].localPosition = pos4;
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

    /// <summary>
    /// Retrieve the cards in the hand based on the children of the cards container.
    /// </summary>
    /// <returns>List of cards Transforms</returns>
    public List<Transform> GetWorldCards()
    {
        if (cardsContainer == null || cardsContainer.childCount == 0) return new List<Transform>();

        List<Transform> children = new List<Transform>();

        foreach (Transform t in cardsContainer)
            children.Add(t);

        return children;
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
