using UnityEngine;
using System.Collections.Generic;
using System;

public class BankruptcyManager : MonoBehaviour
{
    public static BankruptcyManager Instance { get; private set; }
    public PlayerScript bankruptedPlayer { get; private set; }
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void InitiateBankruptcy(PlayerScript player)
    {
        bankruptedPlayer = player;
        var detailPanel = GameManager.Instance.uiManager.detailPanel;
        detailPanel.gameObject.SetActive(true);
        var bankruptcyPanel = detailPanel.transform.Find("BankruptcyPanel");
        bankruptcyPanel.gameObject.SetActive(true);
        var contentPanel = bankruptcyPanel.transform.Find("Scroll View/Viewport/Content");
        // var bankruptcyCardPrefab = 
        

        // mevcut bankruptcy mantığı...
    }

    public void SellProperty(TileRuntimeData tile)
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
        // UpdatePlayerMoneyDisplay();

        // Eğer oyuncu yeterli parayı topladıysa, bankruptcy durumundan çık
        // CheckBankruptcyResolution();
    }

    private void CheckBankruptcyResolution()
    {
        if (bankruptedPlayer.money >= 0)
        {
            // Bankruptcy durumundan çık
            // GameManager.Instance.uiManager.CloseBankruptcyUI();
        }
        else
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
            GameManager.Instance.players.Remove(bankruptedPlayer);
            Destroy(bankruptedPlayer.gameObject);
            // GameManager.Instance.uiManager.CloseBankruptcyUI();
        }
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
}
