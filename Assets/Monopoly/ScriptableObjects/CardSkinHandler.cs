using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CardSkinHandler : MonoBehaviour
{
    public Image backgroundSprite;
    private TextMeshProUGUI nameText;
    private int myID;
    private CustomizeManager manager;

    public void Setup(CardSkinData data, CustomizeManager manager_)
    {
        myID = data.skinID;
        backgroundSprite.sprite = data.backgroundSprite;
        // nameText.text = data.skinName;
        manager = manager_;
    }

    public void OnClick() // Butonun OnClick eventine bunu bağla
    {
        manager.SelectSkin(myID);
    }
}