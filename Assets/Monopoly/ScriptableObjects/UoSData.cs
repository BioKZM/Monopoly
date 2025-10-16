using UnityEngine;

[CreateAssetMenu(fileName = "UoSData", menuName = "Monopoly/UoS Data")]
public class UoSData : TileData
{
    [Header("Satın Alma ve Kira")]
    [SerializeField] public int price;
    [SerializeField] public int rent;
    [SerializeField] public string groupColor;


    private void OnEnable()
    {
        tileName = this.name;
    }
} 