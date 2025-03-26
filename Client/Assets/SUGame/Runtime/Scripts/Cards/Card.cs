using UnityEngine;

public class Card : MonoBehaviour
{
    public string cardName; // 卡牌名称
    private Vector3 originalPosition; // 原始位置
    private bool isSelected; // 是否被选中

    private void Start() => originalPosition = transform.position;

    // 鼠标悬停时触发
    private void OnMouseEnter() => HoverCard(true);
    
    // 鼠标离开时触发
    private void OnMouseExit() => HoverCard(false);

    // 点击卡牌时触发
    private void OnMouseDown() => GameManager.Instance.SelectCard(this);

    // 悬停/取消悬停效果
    private void HoverCard(bool isHovering)
    {
        if (isSelected) return;
        transform.position = originalPosition + (isHovering ? Vector3.up * 0.5f : Vector3.zero);
    }

    // 选中/取消选中
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        transform.position = originalPosition + (selected ? Vector3.up * 1f : Vector3.zero);
    }
}