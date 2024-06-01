using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoosterPack", menuName = "Project Divine/Cards/BoosterPack")]
public class BoosterPackSO : ScriptableObject
{
    public int ID;
    public string PackName;
    public string PackDescription;
    public int PackCost;
    public Sprite PackIcon;
    public List<BoosterPackCardList> cardLists;
    public int totalCards;

    public List<CardDataSO> OpenPack()
    {
        List<CardDataSO> cards = new List<CardDataSO>();
        foreach (BoosterPackCardList cardList in cardLists)
        {
            
            for (int i = 0; i < cardList.minCards; i++)
            {
                cards.Add(cardList.GetCard());
            }
        }
        
        // Add the remaining cards if the total cards is not reached. Add it from random card list.
        while (cards.Count < totalCards)
        {
            int randomCardListIndex = Random.Range(0, cardLists.Count);
            cards.Add(cardLists[randomCardListIndex].GetCard());
        }

        // Shuffle the cards
        for (int i = 0; i < cards.Count; i++)
        {
            CardDataSO temp = cards[i];
            int randomIndex = Random.Range(i, cards.Count);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }

        return cards;
    }
}

[System.Serializable]
public class BoosterPackCardList
{
    public string cardListName;
    public List<CardProbability> cardsProbability;
    public int minCards;
    // public int maxCards;

    public CardDataSO GetCard()
    {
        int totalProbability = 0;
        foreach (CardProbability card in cardsProbability)
        {
            totalProbability += card.probability;
        }
        int randomValue = Random.Range(0, totalProbability);
        int currentProbability = 0;
        foreach (CardProbability card in cardsProbability)
        {
            currentProbability += card.probability;
            if (randomValue < currentProbability)
            {
                return card.card;
            }
        }
        return null;
    }
}

[System.Serializable]
public class CardProbability
{
    public int probability;
    public CardDataSO card;
}