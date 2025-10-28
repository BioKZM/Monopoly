using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Properties;

[RequireComponent(typeof(Button))]
public class PropertyCard : MonoBehaviour
{
    public TileRuntimeData tileData { get; private set; }
    private TextMeshProUGUI currentValueText;
    private TextMeshProUGUI mortgageValueText;

    private void Awake()
    {
    }

    // public void Initialize(TileRuntimeData tile)
    // {
    //     tileData = tile;
    //     UpdateUI();
    // }

    // private void UpdateUI()
    // {
    //     int currentValue = CalculateCurrentValue();
    //     currentValueText.text = "CV: " + currentValue.ToString();
    //     mortgageValueText.text = "MV: " + (currentValue * 70 / 100).ToString();
    // }

    // private void OnCardClick()
    // {
    //     // Animasyon ekleyebilirsiniz
    //     SellProperty();
    // }

    private void SellProperty()
    {

        BankruptcyManager.Instance.SellProperty(tileData);
        Destroy(gameObject);
    }

    private int CalculateCurrentValue()
    {
        int baseValue = 0;
        
        if (tileData.tileData is PropertyData propertyData)
        {
            baseValue += propertyData.price + propertyData.houseCost;
            if (tileData.hasHotel) baseValue += propertyData.hotelCost;
        }
        else if (tileData.tileData is UoSData uOsData)
        {
            baseValue += uOsData.price;
        }
        return baseValue;
    }
}