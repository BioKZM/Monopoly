using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "ChanceCardEffects/GainMoney")]
public class GainMoneyEffect : CardData
{
    public int amount;
    public bool payForBuildings;

    public override void Execute(PlayerScript player)
    {
        if (payForBuildings)
        {
            int totalHouseAmount = 0;
            int totalHotelAmount = 0;
            var ownedTiles = GameManager.Instance.propertyManager.tileRuntimeList.Where(tile => tile.owner == player).ToList();
            foreach (var tile in ownedTiles)
            {
                if (tile.hasHotel)
                {
                    totalHotelAmount++;
                }
                ;
                if (tile.hasHouse)
                {
                    totalHouseAmount++;
                }
            }
            player.money -= (totalHouseAmount * 250) + (totalHotelAmount * 1000);
        }
        else
        {
            player.money += amount;
        }
    }
}