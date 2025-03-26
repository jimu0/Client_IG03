using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;
    public Transform handArea; // 手牌排列区域
    public float cardSpacing = 1.5f; // 卡牌间距

    private List<Card> handCards = new List<Card>();

    private void Awake() => Instance = this;

    // 添加卡牌到手牌并排列
    public void AddCard(Card card)
    {
        handCards.Add(card);
        card.transform.SetParent(handArea);
        card.gameObject.SetActive(true);
        ArrangeCards();
    }

    // 移除卡牌
    public void RemoveCard(Card card)
    {
        handCards.Remove(card);
        ArrangeCards();
    }

    // 排列手牌
    private void ArrangeCards()
    {
        float totalWidth = (handCards.Count - 1) * cardSpacing;
        Vector3 startPos = handArea.position - Vector3.right * totalWidth / 2;

        for (int i = 0; i < handCards.Count; i++)
        {
            Vector3 targetPos = startPos + Vector3.right * (i * cardSpacing);
            handCards[i].transform.position = targetPos;
            handCards[i].transform.localRotation =Quaternion.Euler(0,0,0);
        }
    }
}