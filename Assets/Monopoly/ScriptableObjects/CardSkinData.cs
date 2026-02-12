using UnityEngine;

[CreateAssetMenu(fileName = "NewCardSkin", menuName = "Monopoly/Card Skin")]
public class CardSkinData : ScriptableObject
{
    public int skinID;
    public string skinName;
    public Sprite backgroundSprite;

}