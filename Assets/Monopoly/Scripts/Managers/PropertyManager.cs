using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PropertyManager : MonoBehaviour
{
    #region References
    public List<TileRuntimeData> tileRuntimeList = new List<TileRuntimeData>();
    public List<GameObject> propertyTiles = new List<GameObject>();
    #endregion

    #region Property Management
    public TileRuntimeData GetRuntimeTile(int index)
    {
        return tileRuntimeList[index];
    }
    public TileRuntimeData GetRuntimeTileByName(string tileName)
    {
        return tileRuntimeList.FirstOrDefault(tile => tile.tileData.tileName == tileName);
    }

    public bool HasFullColorSet(PlayerScript player, string colorGroup)
    {
        var groupCount = GetColorGroupCount(colorGroup);
        var propertiesInColorGroup = tileRuntimeList
            .Where(tile => tile.tileData is PropertyData property && property.groupColor == colorGroup)
            .ToList();

        return propertiesInColorGroup.Count == groupCount &&
               propertiesInColorGroup.All(tile => tile.owner == player);
    }
    // public int CountUoSTiles(PlayerScript player, string colorGroup)
    // {
    //     var groupCount = GetColorGroupCount(colorGroup);
    //     var propertiesInColorGroup = tileRuntimeList
    //         .Where(tile => tile.tileData is PropertyData property && property.groupColor == colorGroup)
    //         .ToList();

    //     // return propertiesInColorGroup.Count == groupCount &&
    //     //        propertiesInColorGroup.All(tile => tile.owner == player);
    //     return propertiesInColorGroup.Count(tile => tile.owner == player);
    // }

    private int GetColorGroupCount(string colorGroup)
    {
        return colorGroup switch
        {
            "Brown" or "Blue" => 2,
            "Light Blue" or "Pink" or "Orange" or "Red" or "Yellow" or "Green" => 3,
            "Railroad" => 4,
            "Utility" => 2,
            _ => 0,
        };
    }
    public List<string> GetAllColorGroups()
    {
        HashSet<string> colorGroups = new HashSet<string>
        {
            "Brown",
            "Light Blue",
            "Pink",
            "Orange",
            "Red",
            "Yellow",
            "Green",
            "Blue",
            "Railroad",
            "Utility",
        };

        return colorGroups.ToList();
    }

    public List<List<int>> GetAllRows()
    {
        return new List<List<int>>
        {
            new List<int> {1, 3, 5, 6, 8, 9}, 
            new List<int> {11, 12, 13, 14, 15, 16, 18, 19},
            new List<int> {21,23, 24, 25, 26, 27, 28, 29},
            new List<int> {31, 32, 34, 35, 37, 39},
        };
    }
    public Color GetTileColor(TileData tileData)
    {
        if (tileData is PropertyData propertyData)
        {
            return propertyData.groupColor switch
            {
                "Brown" => new Color(0.1764706f, 0.01960784f, 0.01568628f),
                "Light Blue" => new Color(0.5686275f, 0.7450981f, 0.8196079f),
                "Pink" => new Color(0.8117648f, 0.01568628f, 0.6784314f),
                "Orange" => new Color(0.8039216f, 0.5803922f, 0.007843138f),
                "Red" => new Color(0.8078432f, 0.07843138f, 0.07058824f),
                "Yellow" => new Color(0.509434f, 0.4867925f, 0.0f),
                "Green" => new Color(0.1278409f, 0.5f, 0f),
                "Blue" => new Color(0.02745098f, 0.05490196f, 0.2588235f),

                _ => new Color32(0xA9, 0xA9, 0xA9, 255)
            };
        }
        else if (tileData is UoSData uoSData)
        {
            return uoSData.groupColor switch
            {
                "Railroad" => new Color(0.0f, 0.0f, 0.0f),
                "Utility" => new Color(0.1607843f, 0.1607843f, 0.1607843f),
                _ => new Color32(0xA9, 0xA9, 0xA9, 255)
            };
        }
        else if (tileData is TaxData)
        {   
            return tileData.tileName switch
            {
                "GelirVergisi" => new Color32(0x00, 0x50, 0x30, 255),
                "LüksVergisi" => new Color32(0x00, 0x55, 0x6C, 255),
                _ => new Color32(0xA9, 0xA9, 0xA9, 255)
            };
        }
        return Color.black;
    }

    public bool IsTilePurchasable(TileRuntimeData tile)
    {
        return (tile.tileData is PropertyData || tile.tileData is UoSData) && tile.owner == null;
    }

    public void BuyTile(int buildings)
    {
        // 0 - Sadece arsa
        // 1 - Ev inşa et
        // 2 - Otel inşa Et

        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
        TileRuntimeData currentTile = tileRuntimeList[currentPlayer.currentTileIndex];


        if (currentTile.tileData is PropertyData property)
        {
            ProcessPropertyPurchase(property, buildings, currentTile);
        }
        else if (currentTile.tileData is UoSData uos)
        {
            ProcessUoSPurchase(uos,currentTile);
        }

        FinalizePurchase(currentTile);
    }
    public void GiveAllTilesToPlayer(PlayerScript newOwner)
    {
        foreach (var tile in tileRuntimeList)
        {
            if (!new[] { 0, 2, 4, 7, 10, 17, 20, 22, 30, 33, 36, 38}.Contains(tile.tileData.tileID))
            {
                tile.owner = newOwner;
                newOwner.ownedTiles.Add(tile.tileData.tileName);
            }
        }
    }

    private void ProcessPropertyPurchase(PropertyData property, int buildings, TileRuntimeData currentTile)
    {
        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
        // TileRuntimeData currentTile = tileRuntimeList[currentPlayer.currentTileIndex];
        currentPlayer.money -= property.price;
        GameManager.Instance.eventManager.ShowPurchase(currentPlayer, currentTile);
        if (buildings == 1)
        {
            GameManager.Instance.ShowBuild(currentPlayer, currentTile, "EV");
            currentPlayer.money -= property.houseCost;
        }
        else if (buildings == 2)
        {
            GameManager.Instance.ShowBuild(currentPlayer, currentTile, "OTEL");
            currentPlayer.money -= property.hotelCost;
        }

        // TileRuntimeData currentTile = tileRuntimeList[currentPlayer.currentTileIndex];
        currentTile.hasHouse = buildings == 1;
        currentTile.hasHotel = buildings == 2;
        PlaceBuildings(propertyTiles[currentPlayer.currentTileIndex], buildings);
    }

    private void ProcessUoSPurchase(UoSData uos, TileRuntimeData currentTile)
    {
        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
        currentPlayer.money -= uos.price;
        GameManager.Instance.eventManager.ShowPurchase(currentPlayer, currentTile);
    }

    private void FinalizePurchase(TileRuntimeData currentTile)
    {
        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
        currentTile.owner = currentPlayer;
        currentPlayer.ownedTiles.Add(currentTile.tileData.tileName);
        currentPlayer.hasMadeDecision = true;
        // currentPlayer.UpdateOwnedTilesUI();
        // GameManager.Instance.AddPropertyCardToUI(currentTile.tileData.tileName);
        
        GameManager.Instance.HandleButtonStates(null);
    }

    private void PlaceBuildings(GameObject currentTile, int buildings)
    {
        if (buildings == 0)
        {
            currentTile.transform.GetChild(0).gameObject.SetActive(false);
            currentTile.transform.GetChild(1).gameObject.SetActive(false);
            currentTile.transform.GetChild(2).gameObject.SetActive(true);
        }
        else if (buildings == 1)
        {
            currentTile.transform.GetChild(0).gameObject.SetActive(true);
            currentTile.transform.GetChild(1).gameObject.SetActive(false);
            currentTile.transform.GetChild(2).gameObject.SetActive(false);
        }
        else if (buildings == 2)
        {
            currentTile.transform.GetChild(0).gameObject.SetActive(false);
            currentTile.transform.GetChild(1).gameObject.SetActive(true);
            currentTile.transform.GetChild(2).gameObject.SetActive(false);
        }
        else return;
        // if (buildings < 5 && buildings > 0)
        // {
        //     for (int x = 0; x < buildings; x++)
        //     {
        //         currentTile.transform.GetChild(x).gameObject.SetActive(true);
        //     }
        // }
        // else if (buildings == 5)
        // {
        //     currentTile.transform.GetChild(4).gameObject.SetActive(true);
        // }
    }
    public List<TileRuntimeData> GetPlayerOwnedTiles(PlayerScript player)
    {
        List<TileRuntimeData> playerOwnedTiles = tileRuntimeList
            .Where(tile => tile.owner == player)
            .ToList();
        return playerOwnedTiles;
    }
    #endregion

        #region UI Management
    // public void ManageBuyoutUI(TileRuntimeData tile)
    // {
    //     PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();

    //     if (tile.tileData is PropertyData property)
    //     {
    //         Transform buyoutButtons = GameManager.Instance.uiManager.GetBuyoutButtons();
    //         for (int x = 0; x < 6; x++)
    //         {
    //             buyoutButtons.transform.GetChild(x).GetComponent<Button>().interactable = false;

    //             if (currentPlayer.money >= property.price + (property.houseCost * x))
    //             {
    //                 buyoutButtons.transform.GetChild(x).GetComponent<Button>().interactable = true;
    //             }
    //         }
    //     }
    //     else if (tile.tileData is UoSData uoSData)
    //     {
    //         Transform purchaseButtons = GameManager.Instance.uiManager.GetPurchaseButtons();
    //         if (currentPlayer.money >= uoSData.price)
    //         {
    //             purchaseButtons.transform.GetChild(0).GetComponent<Button>().interactable = true;
    //         }
    //         else
    //         {
    //             purchaseButtons.transform.GetChild(0).GetComponent<Button>().interactable = false;
    //         }
    //     }
    // }
    #endregion
} 