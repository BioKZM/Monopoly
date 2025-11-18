#nullable enable
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine.TerrainUtils;


public class UIManager : MonoBehaviour
{
    public CanvasGroup rollDiceGroup;
    public CanvasGroup propertyActionGroup;
    public CanvasGroup buildGroup;
    public CanvasGroup drawerGroup;
    public CanvasGroup detailPanel;
    public GameObject bankruptcyCard;
    public GameObject ownershipTextPrefab;
    private bool isDrawerOpen = false;
    public List<GameObject> playerInfoPanels = new List<GameObject>();

    void Start()
    {
        SetupDrawer();
    }

    private void SetupDrawer()
    {
        var drawerRect = drawerGroup.GetComponent<RectTransform>();
        var background = drawerRect.Find("DrawerBackground").GetComponent<RectTransform>();
        float bgWidth = background.rect.width;
        drawerRect.anchoredPosition = new Vector2(bgWidth, 0);
    }


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
        ownershipTextPrefab = uiElements.ownershipTextPrefab;


    }

    public void UpdatePlayersInfo(List<PlayerScript> players)
    {
        for (int x = 0; x < players.Count; x++)
        {
            playerInfoPanels[x].transform.Find("InfoPanel").Find("PlayerMoney").Find("PNText").GetComponent<TextMeshProUGUI>().text = FormatMoney(players[x].money);
        }
    }
    private string FormatMoney(int amount)
    {
        return string.Format("{0:N0}₺", amount);
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

    public void HandleButtonStates(TileRuntimeData? tile)
    {
        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
        switch (tile?.tileData.tileType)
        {
            case TileType.Property:
                PropertyData property = (PropertyData)tile.tileData;
                if (tile.owner == currentPlayer)
                {
                    SetGroup(buildGroup, tile.hasHouse, tile.hasHotel, property:property);
                }
                else
                {
                    
                    SetGroup(propertyActionGroup, tile.hasHouse, tile.hasHotel,property:property);
                }
                break;
            case TileType.Utility:
            case TileType.Station:
                UoSData uoSData = (UoSData)tile.tileData;
                SetGroup(propertyActionGroup,uoS:uoSData);
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
                Debug.Log("Received null");
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
    public void RemovePlayerInfoPanel(int index)
    {
        if (index >= 0 && index < playerInfoPanels.Count)
        {
            GameObject panelToRemove = playerInfoPanels[index];
            playerInfoPanels.RemoveAt(index);
            Destroy(panelToRemove);
        }
    }
    public void SetWinnerUI(string playerName)
    {
        var uiElements = GameManager.Instance.GetUIElements();
        var panel = uiElements.detailPanel.gameObject;
        panel.SetActive(true);
        var winnerPanel = panel.transform.Find("WinPanel").gameObject;
        winnerPanel.SetActive(true);
        var winnerCard = winnerPanel.transform.Find("WinnerCard").gameObject;
        var textComponent = winnerCard.transform.Find("WinnerName").GetComponent<TextMeshProUGUI>();
        textComponent.text = playerName;
    }

    public void ShowUserTiles(int playerIndex)
    {
        var currentPlayer = GameManager.Instance.players[playerIndex];
        var ownershipPanel = playerInfoPanels[playerIndex].transform.Find("OwnershipCards").gameObject;
        Color tileTextColor;
        Color tileBGColor;
        var ownershipContentPanel = ownershipPanel.transform.Find("BG/Scroll View/Viewport/Content");
        if (ownershipContentPanel.childCount > 0)
        {
            foreach (Transform child in ownershipContentPanel)
            {
                Destroy(child.gameObject);
            }
        }
        List<TileRuntimeData> playerTiles = GameManager.Instance.GetPlayerOwnedTiles(currentPlayer);
        foreach (TileRuntimeData tile in playerTiles)
        {
            var ownershipTile = Instantiate(ownershipTextPrefab, ownershipContentPanel, false);
            var tileText = ownershipTile.transform.Find("TileText").GetComponent<TextMeshProUGUI>();
            tileText.text = tile.tileData.tileName;

            tileBGColor = GameManager.Instance.GetTileColor(tile.tileData);
            tileTextColor = GetTextColor(tile);

            ownershipTile.GetComponent<Image>().color = tileBGColor;
            tileText.color = tileTextColor;

        }
        ownershipPanel.SetActive(!ownershipPanel.activeInHierarchy);

    }
    public Color GetTextColor(TileRuntimeData tile)
    {
        Color textColor = new();
        if (tile.tileData is PropertyData pData)
        {
            switch (pData.groupColor)
            {
                case "Brown":
                case "Pink":
                case "Orange":
                case "Red":
                case "Blue":
                    textColor = new Color(0xFF, 0xFF, 0xFF, 255);
                    break;
                case "Light Blue":
                case "Yellow":
                case "Green":
                    textColor = new Color(0x00, 0x00, 0x00, 255);
                    break;
            }
        }
        else if (tile.tileData is UoSData uosData)
        {
            textColor = new Color(0xFF, 0xFF, 0xFF, 255);
        }
        return textColor;
    }






    public void SetGroup(CanvasGroup activeGroup, bool hasHouse = false, bool hasHotel = false, PropertyData? property = null, UoSData? uoS = null)
    {
        Debug.Log($"Gelen grup: {activeGroup}");
        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
        CanvasGroup[] groups = { rollDiceGroup, propertyActionGroup, buildGroup };
        foreach (var g in groups)
        {
            bool isActive = g == activeGroup;
            g.gameObject.SetActive(isActive);
            g.interactable = isActive;
            g.blocksRaycasts = isActive;

        }
        if (property != null)
        {
            // Ev varsa
            if (hasHouse)
            {
                // Otele para yetiyorsa
                if (currentPlayer.money > property.hotelCost)
                {
                    buildGroup.transform.GetChild(0).GetComponent<Button>().interactable = false;
                    buildGroup.transform.GetChild(1).GetComponent<Button>().interactable = true;
                }
                // Otele para yetmiyorsa
                else
                {
                    buildGroup.transform.GetChild(0).GetComponent<Button>().interactable = false;
                    buildGroup.transform.GetChild(1).GetComponent<Button>().interactable = false;
                }
            }
            // Otel varsa
            else if (hasHotel)
            {
                buildGroup.transform.GetChild(0).GetComponent<Button>().interactable = false;
                buildGroup.transform.GetChild(1).GetComponent<Button>().interactable = false;
            }
            // Bina yoksa
            else
            {
                // Eve para yetiyorsa
                if (currentPlayer.money > property.houseCost)
                {
                    buildGroup.transform.GetChild(0).GetComponent<Button>().interactable = true;
                    buildGroup.transform.GetChild(1).GetComponent<Button>().interactable = false;
                }
                else if (currentPlayer.money > property.price && currentPlayer.money < property.houseCost)
                {
                    propertyActionGroup.transform.GetChild(0).GetComponent<Button>().interactable = true;
                    buildGroup.transform.GetChild(0).GetComponent<Button>().interactable = false;
                    buildGroup.transform.GetChild(1).GetComponent<Button>().interactable = false;
                }
                else
                {
                    propertyActionGroup.transform.GetChild(0).GetComponent<Button>().interactable = true;
                    buildGroup.transform.GetChild(0).GetComponent<Button>().interactable = false;
                    buildGroup.transform.GetChild(1).GetComponent<Button>().interactable = false;
                }

            }
        }
        if (uoS != null)
        {
            if (currentPlayer.money > uoS.price)
            {
                propertyActionGroup.transform.GetChild(0).GetComponent<Button>().interactable = true;
            }
            else
            {
                propertyActionGroup.transform.GetChild(0).GetComponent<Button>().interactable = false;
            }
        }
    }
    
    

    private IEnumerator AnimateDrawer(float duration)
    {
        var drawerRect = drawerGroup.GetComponent<RectTransform>();
        var background = drawerRect.Find("DrawerBackground").GetComponent<RectTransform>();

        float width = background.rect.width;
        Vector2 startPos = drawerRect.anchoredPosition;
        Vector2 targetPos = isDrawerOpen ? Vector2.zero : new Vector2(width, 0);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            drawerRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }

        drawerRect.anchoredPosition = targetPos;
    }

    public void AddButtonListeners()
    {
        var uiElements = GameManager.Instance.GetUIElements();
        uiElements.rollDiceButton.onClick.AddListener(() => GameManager.Instance.GetTurnManager().OnRollDice());
        uiElements.buyButton.onClick.AddListener(() => GameManager.Instance.GetPropertyManager().BuyTile(0));
        uiElements.buyHouseButton.onClick.AddListener(() => GameManager.Instance.GetPropertyManager().BuyTile(1));
        uiElements.buyHotelButton.onClick.AddListener(() => GameManager.Instance.GetPropertyManager().BuyTile(2));
        uiElements.passButton.onClick.AddListener(() => PassButton());
        uiElements.buildPassButton.onClick.AddListener(() => PassButton());
        uiElements.drawerButton.onClick.AddListener(() =>
        {
            if (drawerGroup == null) return;

            isDrawerOpen = !isDrawerOpen;
            StopAllCoroutines();
            StartCoroutine(AnimateDrawer(0.3f));
        });


    }

} 