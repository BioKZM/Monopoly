#nullable enable
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class UIManager : MonoBehaviour
{

    public CanvasGroup rollDiceGroup;
    public CanvasGroup propertyActionGroup;
    public CanvasGroup buildGroup;
    public List<GameObject> playerInfoPanels = new List<GameObject>();


    public void InitializeUI()
    {
        var uiElements = GameManager.Instance.GetUIElements();
        rollDiceGroup = uiElements.rollDiceGroup;
        propertyActionGroup = uiElements.propertyActionGroup;
        buildGroup = uiElements.buildGroup;
        playerInfoPanels = uiElements.playerInfoPanels;

    }

    public void UpdatePlayersInfo(List<PlayerScript> players)
    {
        for (int x = 0; x < players.Count; x++)
        {
            playerInfoPanels[x].transform.GetChild(1).Find("PlayerMoney").GetComponent<TextMeshProUGUI>().text = FormatMoney(players[x].money);
        }
    }
    public string FormatMoney(int amount)
    {
        return string.Format("{0:N0}TL", amount);
    }
   
    public void UpdateUI()
    {
        UpdatePlayersInfo(GameManager.Instance.players);
    }
    public void PassButton()
    {
        GameManager.Instance.GetCurrentPlayer().hasMadeDecision = true;
        HandleButtonStates(null);
    }

    public void HandleButtonStates(TileRuntimeData? tileData)
    {
        switch (tileData?.tileData.tileType)
        {
            case TileType.Property:
                if (tileData.hasHotel)
                {
                    SetGroup(rollDiceGroup);
                    break;
                }
                if (tileData.owner == GameManager.Instance.GetCurrentPlayer())
                {
                    SetGroup(buildGroup, tileData.hasHouse, tileData.hasHotel);
                }
                else
                {
                    SetGroup(propertyActionGroup, tileData.hasHouse, tileData.hasHotel);
                }
                break;
            case TileType.Utility:
            case TileType.Station:
                SetGroup(propertyActionGroup);
                break;
            case TileType.Chance:
            case TileType.Community:
                SetGroup(rollDiceGroup);
                break;
            case TileType.Tax:
            case TileType.GoToJail:
            case TileType.Corner:
                SetGroup(rollDiceGroup);
                break;
            default:
                SetGroup(rollDiceGroup);
                break;
        }

    }


    public void ShowRollDice()
    {
        SetGroup(rollDiceGroup);
    }

    public void ShowPropertyAction()
    {
        SetGroup(propertyActionGroup);
    }

    public void ShowBuild()
    {
        SetGroup(buildGroup);
    }

    public void SetGroup(CanvasGroup activeGroup, bool hasHouse = false, bool hasHotel = false)
    {
        CanvasGroup[] groups = { rollDiceGroup, propertyActionGroup, buildGroup };
        foreach (var g in groups)
        {
            bool isActive = g == activeGroup;
            g.alpha = isActive ? 1 : 0;
            g.interactable = isActive;
            g.blocksRaycasts = isActive;

        }
        if (hasHouse)
        {
            buildGroup.transform.GetChild(0).GetComponent<Button>().interactable = false;
            buildGroup.transform.GetChild(1).GetComponent<Button>().interactable = true;
        }
        else if (hasHotel)
        {
            buildGroup.transform.GetChild(0).GetComponent<Button>().interactable = false;
            buildGroup.transform.GetChild(1).GetComponent<Button>().interactable = false;
            
        }
        else
        {
            buildGroup.transform.GetChild(0).GetComponent<Button>().interactable = true;
            buildGroup.transform.GetChild(1).GetComponent<Button>().interactable = false;
        }
    }

    public void AddButtonListeners()
    {
        var uiElements = GameManager.Instance.GetUIElements();
        uiElements.rollDiceButton.onClick.AddListener(() => GameManager.Instance.GetTurnManager().OnRollDice());
        uiElements.buyButton.onClick.AddListener(() => GameManager.Instance.GetPropertyManager().BuyTile(0));
        uiElements.buyHouseButton.onClick.AddListener(() => GameManager.Instance.GetPropertyManager().BuyTile(1));
        uiElements.buyHotelButton.onClick.AddListener(() => GameManager.Instance.GetPropertyManager().BuyTile(2));
        uiElements.passButton.onClick.AddListener(() => PassButton());
        
    }
} 