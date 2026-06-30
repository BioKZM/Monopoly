using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class PropertyManager : NetworkBehaviour
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
                "Brown" => new Color32(0x2B, 0x00, 0x00, 255),
                "Light Blue" => new Color32(0x93, 0xD6, 0xFF, 255),
                "Pink" => new Color32(0xFF, 0x00, 0xBF, 255),
                "Orange" => new Color32(0xFF, 0x9D, 0x00, 255),
                "Red" => new Color32(0xFF, 0x00, 0x00, 255),
                "Yellow" => new Color32(0xFF, 0xEA, 0x00, 255),
                "Green" => new Color32(0x30, 0xCB, 0x01, 255),
                "Blue" => new Color32(0x00, 0x09, 0x41, 255),

                _ => new Color32(0xA9, 0xA9, 0xA9, 255)
            };
        }
        else if (tileData is UoSData uoSData)
        {
            return uoSData.groupColor switch
            {
                "Railroad" => new Color32(0x00, 0x00, 0x00,255),
                "Utility" => new Color32(0x27, 0x28, 0x2F,255),
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
        if (!currentPlayer.isLocalPlayer) return;
        GameManager.Instance.CmdProcessPurchase(buildings, currentPlayer.currentTileIndex);
        GameManager.Instance.HandleButtonStates(null);
        
    }

    public void PlaceBuildings(GameObject currentTile, int buildings, Material playerMaterial, Material playerMaterialDark = null)
    {
        if (buildings == 0)
        {
            
            var flag = currentTile.transform.GetChild(2);
            flag.GetComponentInChildren<Renderer>().material = playerMaterial;
            flag.gameObject.SetActive(true);
        }
        else if (buildings == 1)
        {
            var house = currentTile.transform.GetChild(0);
            foreach (Transform child in house)
            {
                child.GetComponent<Renderer>().material = playerMaterial;
            }
            house.gameObject.SetActive(true);
            
            currentTile.transform.GetChild(1).gameObject.SetActive(false);
            currentTile.transform.GetChild(2).gameObject.SetActive(false);
        }
        else if (buildings == 2)
        {
            var hotel = currentTile.transform.GetChild(1);
            hotel.GetChild(0).GetComponent<Renderer>().material = playerMaterial;
            hotel.GetChild(1).GetComponent<Renderer>().material = playerMaterialDark;
            
            hotel.gameObject.SetActive(true);
            
            currentTile.transform.GetChild(0).gameObject.SetActive(false);
            currentTile.transform.GetChild(2).gameObject.SetActive(false);
        }
        else return;
    }
    public List<TileRuntimeData> GetPlayerOwnedTiles(PlayerScript player)
    {
        List<TileRuntimeData> playerOwnedTiles = tileRuntimeList
            .Where(tile => tile.owner == player)
            .ToList();
        return playerOwnedTiles;
    }

    public int GetPlayerUoSCount(PlayerScript player)
    {
        int count = tileRuntimeList
            .Count(tile => tile.owner == player && tile.tileData is UoSData);
        return count;
    }
    #endregion


} 