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
        player.MoveTo(targetTileIndex, isLookingCurrentTile, isJail, goToSpawn);
    }
}