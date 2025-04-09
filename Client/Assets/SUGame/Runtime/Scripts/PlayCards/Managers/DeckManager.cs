using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public GameObject cardPrefab; // 卡牌预制体
    public Transform deckPosition; // 卡池位置
    private List<Card> deck = new List<Card>();

    private void Awake()
    {
        Instance = this;
        InitializeDeck();
    }

    // 初始化卡组
    private void InitializeDeck()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity);
            Card card = cardObj.GetComponent<Card>();
            card.cardName = "Card_" + i;
            deck.Add(card);
            card.gameObject.SetActive(false);
        }
        ShuffleDeck();
    }

    // 洗牌
    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            Card temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    // 抽一张牌
    public Card DrawCard()
    {
        if (deck.Count == 0) return null;
        Card card = deck[0];
        deck.RemoveAt(0);
        return card;
    }
}