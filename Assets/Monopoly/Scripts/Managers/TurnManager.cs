using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    #region Turn Variables
    private int currentPlayerIndex = 0;
    private PlayerScript currentPlayer;
    private int die1;
    private int die2;
    private int dice;
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
        // currentPlayer.UpdateOwnedTilesUI();
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
            // dice = 2;
            // dice = 3;
            // if (currentPlayer.currentTileIndex == 0)
            // {
            //     dice = 30;
            // }
        }
    }
    


    private IEnumerator MovePlayer()
    {
        yield return players[currentPlayerIndex].MoveCoroutine(dice);
        while (players[currentPlayerIndex].isMoving)
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
            Debug.Log($"{currentPlayer.name} pays rent to {currentTile.owner.name}");
            HandleRentPayment(currentTile);
        }
        else if (GameManager.Instance.IsTilePurchasable(currentTile) || currentTile.owner == currentPlayer)
        {
            GameManager.Instance.HandleButtonStates(currentTile);
            yield return HandlePurchaseOption();
        }
        else if (tileType == TileType.Chance || tileType == TileType.Community)
        {
            GameManager.Instance.HandleChanceOrCommunityTile(currentTile);
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
        // Debug.Log($"Rent to pay: {currentTile.GetRent(dice, false)}");
        // Debug.Log($"Current Player Money before rent: {currentPlayer.money}");
        // Debug.Log($"Owner Money before rent: {currentTile.owner.money}");
        // Debug.Log($"Current Tile Owner: {currentTile.owner.name}");
        // Debug.Log($"Current Tile Name: {currentTile.tileData.tileName}");
        bool hasFullSet = false;
        if (currentTile.tileData is PropertyData property)
        {
            hasFullSet = GameManager.Instance.HasFullColorSet(currentTile.owner, property.groupColor);
        }
        int rent = currentTile.GetRent(dice, hasFullSet);
        currentPlayer.money -= rent;
        currentTile.owner.money += rent;
        // Debug.Log(hasFullSet ? "Full set owned by landlord." : "No full set owned by landlord.");
        // Debug.Log($"Current Player Money after rent: {currentPlayer.money}");
        // Debug.Log($"Owner Money after rent: {currentTile.owner.money}");
    }

    private IEnumerator HandlePurchaseOption()
    {
        currentPlayer.hasMadeDecision = false;
        yield return new WaitUntil(() => currentPlayer.hasMadeDecision);
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