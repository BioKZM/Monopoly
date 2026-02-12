using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "ChanceCardEffects/MoveToTile")]
public class MoveToTileEffect : CardData
{
    public int targetTileIndex;
    public bool isLookingCurrentTile;
    public bool goToJail;
    public bool goToSpawn;
    public override void Execute(PlayerScript player)
    {
        if (!NetworkServer.active) return;
        
        if (goToSpawn)
        {
            player.money += 2000;
        }
        player.MoveTo(targetTileIndex, isLookingCurrentTile, goToJail, goToSpawn);
    }
}