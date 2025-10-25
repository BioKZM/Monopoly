using UnityEngine;

[CreateAssetMenu(menuName = "ChanceCardEffects/MoveToTile")]
public class MoveToTileEffect : CardData
{
    public int targetTileIndex;
    public bool isLookingCurrentTile;
    public bool isJail;
    public bool goToSpawn;
    public override void Execute(PlayerScript player)
    {
        if (goToSpawn)
        {
            player.money += 2000;
        }
        player.MoveTo(targetTileIndex, isLookingCurrentTile, isJail, goToSpawn);
    }
}