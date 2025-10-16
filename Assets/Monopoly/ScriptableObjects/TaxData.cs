using UnityEngine;

[CreateAssetMenu(fileName = "TaxData", menuName = "Monopoly/Tax Data")]
public class TaxData : TileData
{
    [Header("Vergi Bilgisi")]
    [SerializeField] public int price;

    private void OnEnable()
    {
        tileType = TileType.Tax;
        tileName = this.name;

    }
} 