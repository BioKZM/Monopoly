using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCard : MonoBehaviour
{
    public PlayerScript owner;
    
    [SerializeField] private Image cardBackgroundImage; 
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private RawImage avatarImage;
    [SerializeField] private Outline avatarOutline;

    public void SetupCard(PlayerScript player)
    {
        if (player == null) return;
        owner = player;

        // 1. İsim ayarla
        if (nameText != null) nameText.text = player.playerName;

        // 2. SADECE BU OYUNCUNUN indexine göre skin çek ve bu kartın resmine bas
        if (cardBackgroundImage != null)
        {
            CardSkinData skin = GameManager.Instance.GetSkinByID(player.playerBackgroundIndex);
            if (skin != null)
            {
                cardBackgroundImage.sprite = skin.backgroundSprite;
            }
        }

        
        if (avatarOutline != null) avatarOutline.effectColor = player.playerColor;
        if (avatarImage != null && player.steamAvatarTexture != null)
        {
            avatarImage.texture = player.steamAvatarTexture;
            avatarImage.color = Color.white;
        }
    }
}