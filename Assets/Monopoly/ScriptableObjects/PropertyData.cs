using UnityEngine;

[CreateAssetMenu(fileName = "PropertyData", menuName = "Monopoly/Property Data")]
public class PropertyData : TileData
{
    [Header("Temel Bilgiler")]
    [SerializeField] public string groupColor;
    
    [Header("Satın Alma ve Kira")]
    [SerializeField] public int price;
    [SerializeField] public int rent;
    [SerializeField] public int fullSetRent;
    [SerializeField] public int houseRent;
    [SerializeField] public int hotelRent;

    [Header("İnşa Maaliyeti")]
    [SerializeField] public int houseCost;
    [SerializeField] public int hotelCost;

    private void OnEnable()
    {
        tileType = TileType.Property;
        tileName = this.name;
    }
} 