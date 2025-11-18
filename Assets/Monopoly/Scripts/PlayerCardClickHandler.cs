using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCardClickHandler : MonoBehaviour
{
    public void OpenClosePanel()
    {
        int playerCardIndex = transform.GetSiblingIndex();
        GameManager.Instance.SetOwnershipPanel(playerCardIndex);
        Debug.Log($"Tıklanan PlayerCard index: {playerCardIndex}");

    }
}
