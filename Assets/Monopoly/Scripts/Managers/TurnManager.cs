using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class TurnManager : MonoBehaviour
{

    public PlayerScript currentPlayer;
    public IList<PlayerScript> players;

    public bool isLocked = true;
    


    #region Turn Management
    public void StartTurn()
    {
        GameManager.Instance.CmdSetPaused(false);
        SetCameraLock(currentPlayer.transform);
        var uiElements = GameManager.Instance.GetUIElements();
        if (currentPlayer.isLocalPlayer)
        {
            uiElements.rollDiceButton.enabled = true;
            GameManager.Instance.SetGroup(uiElements.rollDiceGroup);
        }
        else
        {
            uiElements.rollDiceButton.enabled = false;
            GameManager.Instance.SetGroup(null);
        }
    }

    public void OnRollDice()
    {
        Debug.Log("[TurnManager] - OnRollDice triggered");
        if (currentPlayer.isLocalPlayer)
        {
            Debug.Log("[TurnManager] - Player is Local");
            GameManager.Instance.CmdSetPaused(true);
            GameManager.Instance.GetUIElements().rollDiceButton.enabled = false;
            GameManager.Instance.CmdRequestRoll();
        }
    }
    #endregion

    #region Player Turn Logic
    public IEnumerator PlayerTurnCoroutine(int diceTotal, int d1, int d2)
    {
        // Debug.Log($"D1: {d1}, D2: {d2}, T: {d1+d2}");
        currentPlayer.hasMadeDecision = false;
        currentPlayer.hasRolledDice = true;

        
        yield return MovePlayer(diceTotal);

        
        yield return HandleTileAction(diceTotal);

        yield return new WaitUntil(() => !currentPlayer.isBankrupt);
        
        currentPlayer.hasRolledDice = false;
        if (NetworkServer.active)
        {
            GameManager.Instance.ServerEndTurn();
        }
        
    }
    
    public void ExecuteJailLogic(PlayerScript player, int d1, int d2)
    {
        bool isDouble = (d1 == d2);
        int playerIndex = GameManager.Instance.players.FindIndex(p => p == player);
        int amount = 500 * GameManager.Instance.turnCount;

        while (player.isInJail)
        {
            if (isDouble)
            {
                player.isInJail = false;
                player.jailRollCount = 0;
                Debug.Log($"[JAIL_LOGIC] isDouble triggered : {d1},{d2}]");
                GameManager.Instance.RpcBroadcastMovement(d1+d2, d1, d2);
            }
            else if (player.jailRollCount >= 3)
            {
                player.money -= amount;
                player.isInJail = false;
                player.jailRollCount = 0;
                Debug.Log($"[JAIL_LOGIC] jailRollCount: {player.jailRollCount}, rollCount exhausted");
                GameManager.Instance.RpcShowBailPayment(playerIndex,amount);
                GameManager.Instance.RpcBroadcastMovement(d1+d2, d1, d2);
            }
            else
            {
                player.jailRollCount++;
                Debug.Log($"[JAIL_LOGIC] jailRollCount: {player.jailRollCount}");
                GameManager.Instance.TargetFailedJailRoll(player.connectionToClient, d1, d2);
                break;
            }
        }
        
        
        
    }


    private IEnumerator MovePlayer(int diceTotal)
    {
        
        yield return currentPlayer.MoveCoroutine(diceTotal);
        while (currentPlayer.isMoving)
        {
            yield return null;
        }
    }
    public void SetCameraLock(Transform target)
    {
        var cameraLock = Camera.main.GetComponent<CameraPlayerLock>();
        if (cameraLock != null)
        {
            cameraLock.SetTarget(target);
            cameraLock.isLocked = isLocked;
        }
    }

    private IEnumerator HandleTileAction(int diceTotal)
    {
        GameManager.Instance.CmdSetPaused(false);   
        TileRuntimeData currentTile = GameManager.Instance.GetRuntimeTile(currentPlayer.currentTileIndex);
        TileType tileType = currentTile.tileData.tileType;
        int currentPlayerIndex = GameManager.Instance.players.IndexOf(currentPlayer);

        if (currentTile.owner != null && currentTile.owner != currentPlayer)
        {
            GameManager.Instance.HandleButtonStates(null);
            HandleRentPayment(currentTile, diceTotal);
        }
        else if (GameManager.Instance.IsTilePurchasable(currentTile) || currentTile.owner == currentPlayer)
        {
            if (currentTile.hasHotel)
            {
                GameManager.Instance.HandleButtonStates(null);
            }
            else
            {
                if (currentPlayer.isLocalPlayer)
                    GameManager.Instance.HandleButtonStates(currentTile); 
    
                if (NetworkServer.active)
                    currentPlayer.hasMadeDecision = false;

                // Satın aldığın istasyonlara tekrar geldiğin zaman
                // orayı pas geçiyoruz.
                if (currentTile.tileData is UoSData)
                {
                    currentPlayer.hasMadeDecision = true;
                }
                
                // yield return new WaitUntil(() => currentPlayer.hasMadeDecision);
                int startTurnIndex = GameManager.Instance.turnCount;
                yield return new WaitUntil(() => currentPlayer.hasMadeDecision || GameManager.Instance.turnCount != startTurnIndex);

                // Eğer tur değiştiği için döngüden çıktıysak, UI'ı temizle ve kaç!
                if (GameManager.Instance.turnCount != startTurnIndex) {
                    GameManager.Instance.HandleButtonStates(null);
                    yield break;
                }
            }
        }
        else if (tileType == TileType.Chance || tileType == TileType.Community)
        {
            if (currentPlayer.isLocalPlayer)
            {
                GameManager.Instance.CmdRequestCard(currentTile.tileData.tileID);
            }
        }
        else if (tileType == TileType.Tax)
        {
            var price = currentTile.GetRent(diceTotal, false);
            if (NetworkServer.active) currentPlayer.money -= price;
            if (NetworkServer.active)
            {
                
                GameManager.Instance.RpcShowTaxPayment(currentPlayerIndex, currentTile.tileData.tileName, price);
            }
        }
        else if (tileType == TileType.GoToJail)
        {
            if (NetworkServer.active)
            {
                currentPlayer.GoToJail();
            }
            
        }
        else
        {
            GameManager.Instance.HandleButtonStates(null);
        }
        currentPlayer.CheckBankruptcy();
        
    }

    private void HandleRentPayment(TileRuntimeData currentTile, int diceTotal)
    {
        int currentPlayerIndex = GameManager.Instance.players.IndexOf(currentPlayer);
        int currentOwnerIndex = GameManager.Instance.players.IndexOf(currentTile.owner);
        if (currentTile.owner == null) return;

        bool hasFullSet = false;
        if (currentTile.tileData is PropertyData property)
        {
            hasFullSet = GameManager.Instance.HasFullColorSet(currentTile.owner, property.groupColor);
        }
        int rent = currentTile.GetRent(diceTotal, hasFullSet);
        if (NetworkServer.active)
        {
            currentPlayer.money -= rent;
            currentTile.owner.money += rent;
        }
        GameManager.Instance.RpcShowRentPayment(currentPlayerIndex, currentOwnerIndex, currentTile.tileData.tileName, rent);
    }

    public bool CheckColorSetWin(PlayerScript player)
    {
        var count = 0;
        var propertyManager = GameManager.Instance.GetPropertyManager();
        foreach (var colorGroup in propertyManager.GetAllColorGroups())
        {
            if (propertyManager.HasFullColorSet(player, colorGroup))
            {
                count++;
            }
        }
        if (count >= 3)
        {
            return true;
        }
        return false;
    }
    public bool CheckRowWin(PlayerScript player)
    {
        var propertyManager = GameManager.Instance.GetPropertyManager();
        var columns = propertyManager.GetAllRows();
        foreach (var column in columns)
        {
            bool hasFullColumn = true;
            foreach (var tileIndex in column)
            {
                TileRuntimeData tileData = GameManager.Instance.GetRuntimeTile(tileIndex);
                if (tileData.owner != player)
                {
                    hasFullColumn = false;
                    break;
                }
            }
            if (hasFullColumn)
            {
                return true;
            }
        }
        return false;
    }
    



    #endregion



    #region Getters
    public PlayerScript GetCurrentPlayer()
    {
        return currentPlayer;
    }
    #endregion
    
    public void UpdateUI()
    {
        GameManager.Instance.UpdateUI();
    }
    

} 