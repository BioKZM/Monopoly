using UnityEngine;

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
        Debug.Log(current.name);
        // Parent
        Transform parent = current.transform.parent;
        Debug.Log(parent.name);

        // Parent'ın parent'ı
        Transform grandParent = parent?.parent.parent;
        Debug.Log(grandParent.name);

        // Her ikisini de devre dışı bırak
        if (parent != null)
        {
            parent.gameObject.SetActive(false);
        }

        if (grandParent != null)
        {
            grandParent.gameObject.SetActive(false);
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
