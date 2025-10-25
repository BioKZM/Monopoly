

using NUnit.Framework;

[System.Serializable]
public class TileRuntimeData
{
    public TileData tileData;
    public PlayerScript owner;
    public bool hasHouse = false;
    public bool hasHotel = false;
    
    public int GetRent(int dice, bool hasFullSet, int UoSCount = 0)
    {
        if (tileData is PropertyData property)
        {
            if (hasHotel) return property.hotelRent;
            if (hasHouse) return property.houseRent;
            return property.rent;
        }
        else if (tileData is TaxData tax)
        {
            return tax.price;
        }
        else if (tileData is UoSData UoS)
        {
            if (tileData.tileType == TileType.Station)
            {
                // return hasFullSet ? UoS.rent * 4 : UoS.rent;
                if (hasFullSet)
                {
                    return UoS.rent * 8;
                }
                else if (UoSCount == 3)
                {
                    return UoS.rent * 4;
                }
                else if (UoSCount == 2)
                {
                    return UoS.rent * 2;
                }
                else
                {
                    return UoS.rent;
                }
            }
            else if (tileData.tileType == TileType.Utility)
            {
                return hasFullSet ? UoS.rent * dice * 2 : UoS.rent * dice;
            } 
        }
        return 0;
    }

}
