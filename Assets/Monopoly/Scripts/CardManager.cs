using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    #region Card Data
    public List<CardData> chanceCardEffects;
    public List<CardData> communityCardEffects;
    #endregion

    #region Card Management
    public void HandleChanceOrCommunityTile(TileRuntimeData currentTile)
    {
        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
        
        if (currentTile.tileData.tileType is TileType.Chance)
        {
            int randomIndex = Random.Range(0, chanceCardEffects.Count);
            CardData randomChanceCard = chanceCardEffects[randomIndex];
            randomChanceCard.Execute(currentPlayer);
            // GameManager.Instance.GetUIManager().HandleButtonStates(TileType.Chance);
        }
        else if (currentTile.tileData.tileType is TileType.Community)
        {
            int randomIndex = Random.Range(0, communityCardEffects.Count);
            CardData randomCommunityCard = communityCardEffects[randomIndex];
            randomCommunityCard.Execute(currentPlayer);
            // GameManager.Instance.GetUIManager().HandleButtonStates(TileType.Community);
        }
        currentPlayer.RotatePlayer();
        currentPlayer.hasMadeDecision = true;
    }
    #endregion
} 