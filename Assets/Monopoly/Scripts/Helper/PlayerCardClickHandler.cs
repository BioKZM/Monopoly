using UnityEngine;

public class PlayerCardClickHandler : MonoBehaviour
{
    public void OpenClosePanel()
    {


        int playerCardIndex = transform.GetSiblingIndex();
        PlayerScript player = GameManager.Instance.players[playerCardIndex];
        if (!player.bankrupted)
        {
            GameManager.Instance.SetOwnershipPanel(playerCardIndex);
            Debug.Log($"Tıklanan PlayerCard index: {playerCardIndex}");
            float angle = Mathf.Atan2(player.transform.forward.x, player.transform.forward.z) * Mathf.Rad2Deg;
            var targetRotation = Quaternion.Euler(45f, angle + 45f, 0f);
            GameManager.Instance.turnManager.SetCameraLock(player.transform);
            // StartCoroutine(GameManager.Instance.SmoothCameraMove(player.transform.position + new Vector3(0,5,0),targetRotation, 15, 0.5f));
        }
    }
}
