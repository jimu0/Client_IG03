using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Transform battleField; // 战场区域
    private Card selectedCard; // 当前选中的卡牌

    private void Awake() => Instance = this;

    // 点击卡池抽牌
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Deck"))
            {
                Card card = DeckManager.Instance.DrawCard();
                if (card != null) HandManager.Instance.AddCard(card);
            }
        }
    }

    // 选中卡牌
    public void SelectCard(Card card)
    {
        if (selectedCard != null) selectedCard.SetSelected(false);
        selectedCard = card;
        selectedCard.SetSelected(true);
    }

    // 打出卡牌
    public void PlayCard()
    {
        if (selectedCard == null) return;

        // 生成战场实例
        GameObject battlefieldCard = Instantiate(selectedCard.gameObject, battleField.position, Quaternion.identity);
        battlefieldCard.GetComponent<Card>().enabled = false; // 禁用卡牌交互

        // 移除手牌
        HandManager.Instance.RemoveCard(selectedCard);
        Destroy(selectedCard.gameObject);
        selectedCard = null;
    }

    // 结束回合
    public void EndTurn()
    {
        Debug.Log("回合结束");
        // 重置逻辑
    }
}