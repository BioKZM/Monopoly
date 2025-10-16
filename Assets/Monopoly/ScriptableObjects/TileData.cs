using UnityEngine;

public enum TileType { Property, Utility, Station, Corner, Chance, Tax, Community, GoToJail, None}

[CreateAssetMenu(fileName = "TileData", menuName = "Monopoly/Tile Data")]
public class TileData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    [SerializeField] public string tileName;
    [SerializeField] public int tileID;
    [SerializeField] public TileType tileType;
    private void OnEnable()
    {
        tileName = this.name;
    }

}

