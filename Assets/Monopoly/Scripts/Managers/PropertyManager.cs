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
    public int CountUoSTiles(PlayerScript player, string colorGroup)
    {
        var groupCount = GetColorGroupCount(colorGroup);
        var propertiesInColorGroup = tileRuntimeList
            .Where(tile => tile.tileData is PropertyData property && property.groupColor == colorGroup)
            .ToList();

        // return propertiesInColorGroup.Count == groupCount &&
        //        propertiesInColorGroup.All(tile => tile.owner == player);
        return propertiesInColorGroup.Count(tile => tile.owner == player);
    }

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

                _ => Color.white,
            };
        }
        else if (tileData is UoSData uoSData)
        {
            return uoSData.groupColor switch
            {
                "Railroad" => new Color(0.0f, 0.0f, 0.0f),
                "Utility" => new Color(0.1607843f, 0.1607843f, 0.1607843f),
                _ => Color.white,
            };
        }
        return Color.white;
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
            ProcessPropertyPurchase(property, buildings);
        }
        else if (currentTile.tileData is UoSData uos)
        {
            ProcessUoSPurchase(uos);
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

    private void ProcessPropertyPurchase(PropertyData property, int buildings)
    {
        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
        currentPlayer.money -= property.price;
        if (buildings == 1)
        {
            currentPlayer.money -= property.houseCost;
        }
        else if (buildings == 2)
        {
            currentPlayer.money -= property.hotelCost;
        }

        TileRuntimeData currentTile = tileRuntimeList[currentPlayer.currentTileIndex];
        currentTile.hasHouse = buildings == 1;
        currentTile.hasHotel = buildings == 2;
        PlaceBuildings(propertyTiles[currentPlayer.currentTileIndex], buildings);
    }

    private void ProcessUoSPurchase(UoSData uos)
    {
        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
        currentPlayer.money -= uos.price;
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