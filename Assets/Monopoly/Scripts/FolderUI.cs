using UnityEngine;
using UnityEngine.EventSystems;

public class PropertyCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private float hoverOffset = 10f;

    void Start()
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localPosition = transform.localPosition + new Vector3(0, hoverOffset, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localPosition = transform.localPosition - new Vector3(0, hoverOffset, 0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.ShowPropertyDetails(gameObject.name, GameManager.Instance.GetRuntimeTileByName(gameObject.name));
    }
}