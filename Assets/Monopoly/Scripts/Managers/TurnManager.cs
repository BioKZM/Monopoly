using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR;

public class TurnManager : MonoBehaviour
{
    #region Turn Variables
    private int currentPlayerIndex = 0;
    private PlayerScript currentPlayer;
    private int die1;
    private int die2;
    private int dice;
    public int handDeterminedDice;
    #endregion

    #region References
    public List<PlayerScript> players;
    #endregion

    #region Turn Management
    public void StartTurn()
    {
        currentPlayer = players[currentPlayerIndex];
        SetCameraLock(currentPlayer.transform);

        var uiElements = GameManager.Instance.GetUIElements();
        uiElements.rollDiceButton.enabled = true;
        GameManager.Instance.SetGroup(uiElements.rollDiceGroup);
    }

    public void OnRollDice()
    {
        var uiElements = GameManager.Instance.GetUIElements();
        uiElements.rollDiceButton.enabled = false;
        StartCoroutine(PlayerTurnCoroutine());
    }

    public void EndTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        GameManager.Instance.UpdateUI();
        CheckWinConditions();
        StartTurn();
    }
    #endregion

    #region Player Turn Logic
    public IEnumerator PlayerTurnCoroutine()
    {
        RollDice();
        if (!currentPlayer.isInJail)
        {
            yield return MovePlayer();
            yield return HandleTileAction();
            yield return new WaitUntil(() => !currentPlayer.isBankrupt);
            EndTurn();
        }
    }
    
    private void RollDice()
    {
        var uiElements = GameManager.Instance.GetUIElements();
        if (currentPlayer.isInJail)
        {
            die1 = Random.Range(1, 7);
            die2 = Random.Range(1, 7);
            if (die1 == die2)
            {
                currentPlayer.isInJail = false;
                dice = die1 + die2;
                uiElements.rollDiceButton.enabled = false;
                
            }
            else
            {
                currentPlayer.jailRollCount++;
                uiElements.rollDiceButton.enabled = true;
                if (currentPlayer.jailRollCount >= 3)
                {
                    currentPlayer.money -= 5000;
                    currentPlayer.isInJail = false;
                    uiElements.rollDiceButton.enabled = false;
                    dice = die1 + die2;
                   
                }
            }
        }
        else
        {
            uiElements.rollDiceButton.enabled = false;
            die1 = Random.Range(1, 7);
            die2 = Random.Range(1, 7);
            dice = die1 + die2;
            if (handDeterminedDice > 0)
            {
                dice = handDeterminedDice;
                // handDeterminedDice = 0;
                return;
            }
            
        }
    }
    


    private IEnumerator MovePlayer()
    {
        yield return currentPlayer.MoveCoroutine(dice);
        while (currentPlayer.isMoving)
        {
            yield return null;
        }
    }
    private void SetCameraLock(Transform target)
    {
        var cameraLock = Camera.main.GetComponent<CameraPlayerLock>();
        if (cameraLock != null)
        {
            cameraLock.SetTarget(target);
        }
    }

    private IEnumerator HandleTileAction()
    {
        TileRuntimeData currentTile = GameManager.Instance.GetRuntimeTile(currentPlayer.currentTileIndex);
        TileType tileType = currentTile.tileData.tileType;

        if (currentTile.owner != null && currentTile.owner != currentPlayer)
        {
            GameManager.Instance.HandleButtonStates(null);
            HandleRentPayment(currentTile);
        }
        else if (GameManager.Instance.IsTilePurchasable(currentTile) || currentTile.owner == currentPlayer)
        {
            // if (currentTile.tileData is PropertyData property)
            // {
            //     if (currentTile.hasHotel)
            //     {
            //         GameManager.Instance.HandleButtonStates(null);
            //     }
            //     else if (currentPlayer.money > property.price)
            //     {
            //         GameManager.Instance.HandleButtonStates(currentTile);
            //         yield return HandlePurchaseOption();
            //     }
            //     else if (currentPlayer.money > property.houseCost)
            //     {
            //         GameManager.Instance.HandleButtonStates()
            //     }
            // }
            // else if (currentTile.tileData is UoSData uoS)
            // {
                
            // }
            if (currentTile.hasHotel)
            {
                GameManager.Instance.HandleButtonStates(null);
            }
            else
            {
                GameManager.Instance.HandleButtonStates(currentTile);
                yield return HandlePurchaseOption();
            }
        }
        else if (tileType == TileType.Chance || tileType == TileType.Community)
        {
            GameManager.Instance.HandleChanceOrCommunityTile(currentTile);
        }
        else if (tileType == TileType.Tax)
        {
            var price = currentTile.GetRent(dice, false);
            currentPlayer.money -= price;
            GameManager.Instance.ShowTaxPayment(currentPlayer, currentTile, price);
        }
        else if (tileType == TileType.GoToJail)
        {
            currentPlayer.GoToJail();
            
        }
        else
        {
            GameManager.Instance.HandleButtonStates(null);
        }
        currentPlayer.CheckBankruptcy();
        currentPlayer.RotatePlayer();
    }

    private void HandleRentPayment(TileRuntimeData currentTile)
    {
        bool hasFullSet = false;
        if (currentTile.tileData is PropertyData property)
        {
            hasFullSet = GameManager.Instance.HasFullColorSet(currentTile.owner, property.groupColor);
        }
        int rent = currentTile.GetRent(dice, hasFullSet);
        currentPlayer.money -= rent;
        currentTile.owner.money += rent;
        GameManager.Instance.ShowRentPayment(currentPlayer, currentTile.owner, currentTile, rent);
    }

    private IEnumerator HandlePurchaseOption()
    {
        currentPlayer.hasMadeDecision = false;
        yield return new WaitUntil(() => currentPlayer.hasMadeDecision);
    }


    private void CheckWinConditions()
    {
        if (CheckLastPlayerStanding())
        {
            GameManager.Instance.HandleWin(players[0]);
            return;
        }
        foreach (var player in players)
        {

            if (CheckColorSetWin(player) || CheckRowWin(player))
            {
                GameManager.Instance.HandleWin(player);
                break;
            }
        }
    }

    private bool CheckLastPlayerStanding()
    {
        if (players.Count == 1)
        {
            return true;
        }
        return false;
    }
    private bool CheckColorSetWin(PlayerScript player)
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
    private bool CheckRowWin(PlayerScript player)
    {
        var propertyManager = GameManager.Instance.GetPropertyManager();
        var columns = propertyManager.GetAllRows();
        foreach (var column in columns)
        {
            bool hasFullColumn = true;
            foreach (var tileIndex in column)
            {
                TileRuntimeData tileData = GameManager.Instance.GetRuntimeTile(tileIndex);
                // tileData.tileData.tileType != TileType.Property ||
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

    public int GetCurrentPlayerIndex()
    {
        return currentPlayerIndex;
    }

    public int GetDice()
    {
        return dice;
    }
    #endregion
} 