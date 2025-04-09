using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    private void OnMouseDown() => GameManager.Instance.EndTurn();
}