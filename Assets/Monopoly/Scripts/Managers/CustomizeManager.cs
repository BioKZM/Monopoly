using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeManager : MonoBehaviour
{
    public List<CardSkinData> allSkins; // Editörden 40 taneyi buraya sürükle
    public GameObject cardPrefab; // Yukarıdaki prefab
    public Transform contentParent;
    public Image previewCardImage;     // ScrollView Content'i

    void Start()
    {
        foreach (var skin in allSkins)
        {
            GameObject go = Instantiate(cardPrefab, contentParent);
            go.transform.localScale = new Vector3(2f, 2f, 2f);
            go.GetComponent<CardSkinHandler>().Setup(skin, this);
        }
    }

    public void SelectSkin(int id)
    {
        PlayerPrefs.SetInt("SelectedCardSkin", id);
        PlayerPrefs.Save();
        CardSkinData selectedData = allSkins.Find(x => x.skinID == id);
        if (selectedData != null)
        {
            // Önizleme kartındaki Image bileşeni
            previewCardImage.sprite = selectedData.backgroundSprite; 
        }
    }
}