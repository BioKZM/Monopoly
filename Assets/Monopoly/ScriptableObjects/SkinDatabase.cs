using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinDatabase", menuName = "Monopoly/Skin Database")]
public class SkinDatabase : ScriptableObject
{
    public List<CardSkinData> allSkins;

    public CardSkinData GetSkinByID(int id)
    {
        return allSkins.Find(x => x.skinID == id);
    }
}