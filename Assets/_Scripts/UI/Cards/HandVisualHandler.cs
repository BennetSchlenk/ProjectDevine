using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Hand))]
public class HandVisualHandler : MonoBehaviour
{
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


    private HandController handController;
    private BezierCurve _bezierCurve;
    private Transform cardsContainer;

    // Drag & Drop
    private bool isDragging = false;
    private CardMovement draggingCard;





    private void Awake()
    {
        handController = GetComponent<HandController>();
        _bezierCurve = GetComponentInChildren<BezierCurve>();
    }

    // Start is called before the first frame update
    void Start()
    {

        cardsContainer = handController.CardsContainer;
        OrderCardsInWorld();
        handController.OnHandUpdate += OnHandUpdate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        handController.OnHandUpdate -= OnHandUpdate;

        OnCardDraggedAction = null;
        OnCardDroppedAction = null;
        OnCardRemoveAction = null;
    }

    [ContextMenu("Order Cards in World")]
    public void OrderCardsInWorld()
    {

        List<Transform> children = GetWorldCards();

        if (children == null) return;

        // If children.Count is 0, then there is no need to reorder
        if (children.Count == 0) return;
        // If children.Count is 1, then there is no need to reorder
        if (children.Count == 1)
        {
            children[0].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.5f), _bezierCurve.ControlPoints[3].rotation, initialPlaceTime);
        } else if (children.Count == 2)
        {
            children[0].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.25f), _bezierCurve.GetCardOrientation(0.25f), initialPlaceTime);
            children[1].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.75f), _bezierCurve.GetCardOrientation(0.75f), initialPlaceTime);
        } else if (children.Count == 3)
        {
            children[0].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.2f), _bezierCurve.GetCardOrientation(0.2f), initialPlaceTime);
            children[1].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.5f), _bezierCurve.GetCardOrientation(0.5f), initialPlaceTime);
            children[2].GetComponent<CardMovement>().MoveToPosition(_bezierCurve.GetBezierPoint(0.8f), _bezierCurve.GetCardOrientation(0.8f), initialPlaceTime);
        }
        
        
        else
        {
            for (int i = 0; i < children.Count; i++)
            {
                Vector3 cardFinalPosition = _bezierCurve.GetBezierPoint((float)i / (children.Count-1));
                Quaternion cardFinalRotation = _bezierCurve.GetCardOrientation((float)i / (children.Count-1));

                CardMovement cardMovement = children[i].GetComponent<CardMovement>();

                cardMovement.MoveToPosition(cardFinalPosition, cardFinalRotation, reorderTime);
                
            }
        }

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

    private List<Transform> GetWorldCards()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform t in cardsContainer)
        {
            children.Add(t);
        }

        return children;
    }

    private void OnHandUpdate()
    {
        Debug.Log("OnHandUpdate()");
        OrderCardsInWorld();
    }

    private void OnCardRemove(CardMovement cardMovement)
    {
        cardMovement.OnCardDragged -= OnCardDragged;
        cardMovement.OnCardDropped -= OnCardDropped;
        cardMovement.OnCardRemove -= OnCardRemove;
        cardMovement.OnCardClicked -= OnCardClicked;
        OnCardRemoveAction(cardMovement);
    }

    private void OnCardClicked(CardMovement cardMovement)
    {
        Debug.Log("Clicked card: " + cardMovement.gameObject.name);
        OnCardClickedAction(cardMovement);
    }


    #region Drag & Drop

    private void OnCardDragged(CardMovement cardMovement)
    {
        isDragging = true;
        draggingCard = cardMovement;
        //Debug.Log("Dragging card: " + cardMovement.gameObject.name);
        OnCardDraggedAction(cardMovement);
    }

    private void OnCardDropped(CardMovement cardMovement)
    {
        Debug.Log("Dropped card in: " + cardMovement.gameObject.name);
        OnCardDroppedAction(cardMovement);
    }

    #endregion

    
}
