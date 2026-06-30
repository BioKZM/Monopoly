using UnityEngine;
using UnityEngine.UI;

public class CloseDetailScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnButtonClick()
    {
        // Mevcut butonun GameObject'i
        GameObject current = gameObject;
        
        // Parent
        Transform parent = current.transform.parent;

        // Parent'ın parent'ı
        Transform grandParent = parent?.parent.parent;

        // Her ikisini de devre dışı bırak
        if (parent != null)
        {
            PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
            // StartCoroutine(GameManager.Instance.SmoothCameraMove(resetCamera:true));
            var cam = Camera.main;
            var cameraLock = cam.GetComponent<CameraPlayerLock>();
            if (cameraLock != null) cameraLock.enabled = true;
            GameManager.Instance.turnManager.SetCameraLock(currentPlayer.transform);
            parent.gameObject.SetActive(false);
        }

        if (grandParent != null)
        {
            grandParent.gameObject.SetActive(false);
            grandParent.GetComponent<Image>().color = new Color(0,0,0,0.972549f);
            
        }
    }
    public void CloseCardUI()
    {
        GameObject current = gameObject;
        Transform parent = current.transform.parent;
        Transform grandParent = parent?.parent;
        if (parent != null)
        {
            parent.gameObject.SetActive(false);
        }
        if (grandParent != null)
        {
            grandParent.gameObject.SetActive(false);
            
        }
    }
}
