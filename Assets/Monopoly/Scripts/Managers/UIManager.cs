#nullable enable
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Properties;


public class UIManager : MonoBehaviour
{

    public CanvasGroup rollDiceGroup;
    public CanvasGroup propertyActionGroup;
    public CanvasGroup buildGroup;
    public CanvasGroup drawerGroup;
    public CanvasGroup detailPanel;
    public GameObject bankruptcyCard;
    private bool isDrawerOpen = false;
    public List<GameObject> playerInfoPanels = new List<GameObject>();


    public void InitializeUI()
    {
        var uiElements = GameManager.Instance.GetUIElements();
        rollDiceGroup = uiElements.rollDiceGroup;
        propertyActionGroup = uiElements.propertyActionGroup;
        buildGroup = uiElements.buildGroup;
        playerInfoPanels = uiElements.playerInfoPanels;
        drawerGroup = uiElements.drawerGroup;
        detailPanel = uiElements.detailPanel;
        bankruptcyCard = uiElements.bankruptcyCard;



    }

    public void UpdatePlayersInfo(List<PlayerScript> players)
    {
        for (int x = 0; x < players.Count; x++)
        {
            playerInfoPanels[x].transform.Find("InfoPanel").Find("PlayerMoney").Find("PNText").GetComponent<TextMeshProUGUI>().text = FormatMoney(players[x].money);
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
    public void ShowPropertyDetails(string tileName, TileRuntimeData data)
    {

        if (detailPanel != null)
        {
            detailPanel.gameObject.SetActive(true);
            var panel = detailPanel.transform.GetChild(0);
            var card = panel.transform.Find(tileName);
            card.gameObject.SetActive(true);
            Debug.Log(card.transform.GetChild(1).name);
            card.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = data.owner != null ? data.owner.playerName : null ?? "";
        }
    }
    
    public void CloseDetailPanel()
    {
        detailPanel.gameObject.SetActive(false);
        for (int x = 0; x < detailPanel.transform.childCount;x++)
        {
            if (x == 0) continue;
            detailPanel.transform.GetChild(x).gameObject.SetActive(false);
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
    public void SetupCardUI(string cardText, bool isChanceCard)
    {
        var uiElements = GameManager.Instance.GetUIElements();
        var panel = uiElements.detailPanel.gameObject;
        panel.SetActive(true);
        var card = isChanceCard ? panel.transform.Find("ChanceCards").gameObject : panel.transform.Find("CommunityCards").gameObject;
        card.SetActive(true);
        var textComponent = card.transform.Find("CardText").GetComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = cardText;
    }


    public void SetGroup(CanvasGroup activeGroup, bool hasHouse = false, bool hasHotel = false)
    {
        CanvasGroup[] groups = { rollDiceGroup, propertyActionGroup, buildGroup };
        foreach (var g in groups)
        {
            bool isActive = g == activeGroup;
            g.gameObject.SetActive(isActive);
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
    
    public void RemovePlayerInfoPanel(int index)
    {
        if (index >= 0 && index < playerInfoPanels.Count)
        {
            GameObject panelToRemove = playerInfoPanels[index];
            playerInfoPanels.RemoveAt(index);
            Destroy(panelToRemove);
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
        uiElements.drawerButton.onClick.AddListener(() =>
        {
            // drawerGroup.transform.position = isDrawerOpen ? drawerGroup.transform.position + new Vector3(375, 0, 0) : drawerGroup.transform.position - new Vector3(375, 0, 0);
            Vector3 targetPosition = isDrawerOpen ? 
            drawerGroup.transform.position + new Vector3(375, 0, 0) : 
            drawerGroup.transform.position - new Vector3(375, 0, 0);
            drawerGroup.transform.position = targetPosition;
            // drawerGroup.transform.position = Vector3.Lerp(
            //     drawerGroup.transform.position,
            //     targetPosition,
            //     Time.deltaTime * 5f  // Hız faktörü
            // );
            isDrawerOpen = !isDrawerOpen;
        });


    }

} 