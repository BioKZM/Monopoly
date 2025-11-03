using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;

public class BankruptcyManager : MonoBehaviour
{
    public static BankruptcyManager Instance { get; private set; }
    public PlayerScript bankruptedPlayer { get; private set; }
    private CanvasGroup detailPanel;
    private Transform bankruptcyPanel;
    
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
    }
    private void Start()
    {
        detailPanel = GameManager.Instance.uiManager.detailPanel;
        bankruptcyPanel = detailPanel.transform.Find("BankruptcyPanel");
        bankruptcyPanel.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(() => CheckBankruptcyResolution());
    }

    public void InitiateBankruptcy(PlayerScript player)
    {
        bankruptedPlayer = player;
        detailPanel.gameObject.SetActive(true);
        bankruptcyPanel.gameObject.SetActive(true);
        
        var contentPanel = bankruptcyPanel.transform.Find("Scroll View/Viewport/Content");
        var bankruptcyCardPrefab = GameManager.Instance.uiManager.bankruptcyCard;
        // GameObject prefab = Resources.Load<GameObject>("Assets/Monopoly/Prefabs/BankruptcyCard.prefab");
        foreach (var tileName in bankruptedPlayer.ownedTiles)
        {
            var tile = GameManager.Instance.propertyManager.tileRuntimeList.Find(t => t.tileData.tileName == tileName);
            if (tile != null)
            {
                Color tileColor = GameManager.Instance.propertyManager.GetTileColor(tile.tileData);
                int mortgageValue = CalculateMortgageValue(tile);
                int currentValue = CalculateCurrentValue(tile);
                GameObject instance = Instantiate(bankruptcyCardPrefab, contentPanel, false);
                instance.transform.Find("ColorPanel").GetComponent<Image>().color = tileColor;
                Transform tilePanel = instance.transform.Find("ColorPanel/TileName");
                tilePanel.GetComponent<TextMeshProUGUI>().text = tile.tileData.tileName;
                // Transform valuePanel = instance.transform.Find("ValuePanel");

                instance.transform.Find("OGValue").GetComponent<TextMeshProUGUI>().text = currentValue.ToString() + "TL";
                instance.transform.Find("MGValue").GetComponent<TextMeshProUGUI>().text = mortgageValue.ToString() + "TL";
                var sellButton = instance.transform.Find("SellButton").GetComponent<Button>();
                sellButton.onClick.AddListener(() => SellProperty(tile, sellButton));
            }
        }
        
    }

    public void SellProperty(TileRuntimeData tile, Button sellButton=null)
    {
        // Arsayı oyuncudan çıkar
        bankruptedPlayer.ownedTiles.Remove(tile.tileData.tileName);
        
        // Mortgage değerini oyuncuya ekle
        int mortgageValue = CalculateMortgageValue(tile);
        bankruptedPlayer.money += mortgageValue;
        
        // PropertyManager'daki tile'ı güncelle
        int tileIndex = GameManager.Instance.propertyManager.tileRuntimeList.FindIndex(t => t.tileData.tileName == tile.tileData.tileName);
        List<TileRuntimeData> tileRuntimeList = GameManager.Instance.propertyManager.tileRuntimeList;
        if (tileIndex != -1)
        {
            tileRuntimeList[tileIndex].owner = null;
            tileRuntimeList[tileIndex].hasHouse = false;
            tileRuntimeList[tileIndex].hasHotel = false;
        }
        
        // Görsel güncellemeler (evler ve oteller)
        if (GameManager.Instance.propertyTiles.Count > tileIndex)
        {
            GameObject propertyTile = GameManager.Instance.propertyTiles[tileIndex];
            // Tüm ev ve otel görsellerini kapat
            for (int i = 0; i < propertyTile.transform.childCount; i++)
            {
                propertyTile.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        // UI'ı güncelle
        GameManager.Instance.uiManager.UpdateUI();
        if (sellButton != null)
        {
            DeleteCardFromBankruptcyUI(sellButton);
            GameManager.Instance.ShowSelling(bankruptedPlayer, tile);
        }
        // UpdatePlayerMoneyDisplay();

        // Eğer oyuncu yeterli parayı topladıysa, bankruptcy durumundan çık
        // CheckBankruptcyResolution();
    }

    private void CheckBankruptcyResolution()
    {
        if (bankruptedPlayer.money < 0)
        {

            // Oyuncunun kalan arsalarını sat
            foreach (string tileName in bankruptedPlayer.ownedTiles)
            {
                var tile = GameManager.Instance.propertyManager.tileRuntimeList.Find(t => t.tileData.tileName == tileName);
                if (tile != null)
                {
                    SellProperty(tile);
                }
            }

            // Oyuncuyu oyundan çıkar
            GameManager.Instance.ShowBankrupt(bankruptedPlayer);
            GameManager.Instance.RemovePlayerFromGame(bankruptedPlayer);
            // GameManager.Instance.players.Remove(bankruptedPlayer);
            // Destroy(bankruptedPlayer.gameObject);
            // GameManager.Instance.uiManager.CloseBankruptcyUI();
        }
        bankruptedPlayer.isBankrupt = false;
        GameManager.Instance.uiManager.CloseDetailPanel();
        
    }
    

    

    private int CalculateMortgageValue(TileRuntimeData tile)
    {
        return CalculateCurrentValue(tile) * 85 / 100; // %85 mortgage değeri
    }

    // Arsanın mevcut değerini hesapla (evler ve oteller dahil)
    private int CalculateCurrentValue(TileRuntimeData tile)
    {
        int currentValue = 0;

        if (tile.tileData is PropertyData property)
        {
            currentValue += property.price + (tile.hasHouse ? property.houseCost : 0) + (tile.hasHotel ? property.hotelCost : 0);
        }
        else if (tile.tileData is UoSData uOs)
        {
            currentValue = uOs.price;
        }
        return currentValue;

    }
    private void DeleteCardFromBankruptcyUI(Button sellButton)
    {
        Destroy(sellButton.transform.parent.gameObject);
    }
}
