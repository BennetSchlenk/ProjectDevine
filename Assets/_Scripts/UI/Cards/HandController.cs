using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public event Action OnHandUpdate = delegate { };

    [SerializeField] private BezierCurve bezierCurve;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardsContainer;
    public Transform CardsContainer => cardsContainer;

    private List<BezierChildMovement> cards = new();

    #region Unity Callbacks
        
    private void Awake()
    {
        
    }

    private void Start()
    {
        //Refresh();
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
}
