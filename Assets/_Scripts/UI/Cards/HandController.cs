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

    #region Unity Callbacks
        
    private void Awake()
    {
        handVisualHandler = GetComponent<HandVisualHandler>();
        handVisualHandler.OnCardDraggedAction += UseCard;
    }

    private void Start()
    {
        //Refresh();
    }

    private void OnDestroy()
    {
        handVisualHandler.OnCardDraggedAction -= UseCard;
    }

    #endregion

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


    private void UseCard(CardMovement cardMovement)
    {
        Card card = cardMovement.GetComponent<Card>();
        if (card == null) return;

        if (card.Prefab != null)
            objectPlacer.StartPlacing(card.Prefab);


        //card.Use();
        //Refresh();
    }
}
