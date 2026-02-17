#nullable enable
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public CanvasGroup rollDiceGroup;
    public CanvasGroup propertyActionGroup;
    public CanvasGroup buildGroup;
    public CanvasGroup drawerGroup;
    public CanvasGroup detailPanel;
    public GameObject bankruptcyCard;
    public GameObject ownershipTextPrefab;
    public GameObject loadingPanel;
    public GameObject escPanel;
    public TextMeshProUGUI loadingText;
    public Button returnToMainMenuButton;
    public Button quitGameButton;
    // public GameObject logsWindow;
    // public Button openLogsButton;
    public GameObject startGameButton;
    private bool isDrawerOpen = false;
    public List<GameObject> playerInfoPanels = new();



    
    public void UpdateLoadingStatus(int ready, int total)
    {
        if (loadingText != null)
        {
            loadingText.text = $"Oyuncular Hazırlanıyor... ({ready}/{total})";
        }
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
        loadingPanel = uiElements.loadingPanel;
        loadingText = uiElements.loadingText;
        startGameButton = uiElements.startGameButton;
        escPanel = uiElements.escPanel;
        returnToMainMenuButton = uiElements.returnToMainMenuButton;
        quitGameButton = uiElements.quitGameButton;


        SetupDrawer();
    }

    
    public void SetPlayersInfo(IList<PlayerScript> players)
    {
        if (players == null || playerInfoPanels == null) return;

        for (int x = 0; x < players.Count; x++)
        {
            if (x >= playerInfoPanels.Count) {
                Debug.LogWarning($"[UI] {x} indexli oyuncu için UI paneli atanmamış!");
                continue; 
            }

            PlayerScript pScript = players[x];
            Transform panel = playerInfoPanels[x].transform;

            // --- VERİ DOĞRULAMA (KRİTİK KISIM) ---
            // Eğer playerName boşsa veya "Loading..." kalmışsa, visualData'dan çekmeyi dene
            if (string.IsNullOrEmpty(pScript.playerName) || pScript.playerName == "Loading...")
            {
                if (pScript.visualData != null && !string.IsNullOrEmpty(pScript.visualData.steamName))
                {
                    pScript.playerName = pScript.visualData.steamName;
                    pScript.playerColor = pScript.visualData.playerColor;
                }
            }

            // --- UI GÜNCELLEME ---
            // 1. İsim Güncelleme
            Transform nameObj = panel.Find("InfoPanel/PlayerName/PNText");
            if (nameObj != null && nameObj.TryGetComponent(out TextMeshProUGUI txt))
            {
                string playerName = pScript.playerName;
                if (string.IsNullOrEmpty(playerName))
                {
                    playerName = "Loading...";
                }
                else
                {
                    playerName = pScript.playerName;
                }
                if (playerName.Length > 14)
                {
                    playerName = playerName.Substring(0,14);
                    txt.fontSize = 14;
                }
                else
                {
                    switch (playerName.Length)
                    {
                        case 10:
                            txt.fontSize = 22;
                            break;
                        case 11:
                            txt.fontSize = 20;
                            break;
                        case 12:
                            txt.fontSize = 18;
                            break;
                        case 13:
                            txt.fontSize = 16;
                            break;
                        case 14:
                            txt.fontSize = 14;
                            break;
                        default:
                            txt.fontSize = 24;
                            break;
                    }
                }
                
                txt.text = playerName;
            }

            // 2. Renk ve Outline Güncelleme
            Transform outlineObj = panel.Find("AvatarOutline");
            if (outlineObj != null && outlineObj.TryGetComponent(out Outline outline))
            {
                // Eğer renk hala beyaz (varsayılan) ise visualData'dan tekrar kontrol et
                if (pScript.playerColor == Color.white && pScript.visualData != null)
                    pScript.playerColor = pScript.visualData.playerColor;

                outline.effectColor = pScript.playerColor;
            }

            // 3. Avatar Güncelleme
            Transform avatarObj = panel.Find("Avatar");
            if (avatarObj != null && avatarObj.TryGetComponent(out RawImage img))
            {
                if (pScript.steamAvatarTexture != null)
                {
                    img.texture = pScript.steamAvatarTexture;
                    img.color = Color.white;
                }
                else 
                {
                    img.color = new Color(1, 1, 1, 0); 
                }
            }
            
            // 4. Kart arkaplanı güncelleme
            if (panel.TryGetComponent<Image>(out var backgroundSprite))
            {
                CardSkinData skin = GameManager.Instance.GetSkinByID(pScript.playerBackgroundIndex);
                if (skin != null)
                {
                    backgroundSprite.sprite = skin.backgroundSprite;
                }
            }
        }

    // Listenin dolup dolmadığını kontrol et
        if (playerInfoPanels == null || playerInfoPanels.Count == 0) {
            Debug.LogWarning("[UI_MANAGER:181] Paneller henüz hazır değil!");
            return;
        }
        else
        {
            for (int i = 0; i < players.Count; i++)
            {
                // 2. ADIM: Index koruması (Panel sayısı oyuncu sayısından azsa patlama)
                if (i >= playerInfoPanels.Count) break;

                GameObject cardObj = playerInfoPanels[i];
                if (cardObj == null) continue;

                PlayerCard cardScript = cardObj.GetComponent<PlayerCard>();

                // 3. ADIM: Patlayan yerin koruması (İşte burası!)
                if (cardScript != null && players[i] != null)
                {
                    cardScript.owner = players[i]; // 208. satır artık güvende
                    // Diğer UI atamalarını da burada güvenle yapabilirsin
                    // Örn: cardScript.nameText.text = players[i].playerName;
                }
                else 
                {
                    Debug.LogWarning($"Hacı, {i}. indisteki kartta script yok veya oyuncu null!");
                }
            }
        }
        
    }
    public void UpdatePlayersInfo(IList<PlayerScript> players)
    {
        for (int x = 0; x < players.Count; x++)
        {
            playerInfoPanels[x].transform.Find("InfoPanel").Find("PlayerMoney").Find("PNText").GetComponent<TextMeshProUGUI>().text = FormatMoney(players[x].money);
        }
    }
    public string FormatMoney(int amount)
    {
        return string.Format("{0:N0}₺", amount);
    }
   
    public void UpdateUI()
    {
        UpdatePlayersInfo(GameManager.Instance.players);
        SetTurnTimer();
    }

    public void UpdateTimer(float time, bool last5)
    {
        int playerIndex = GameManager.Instance.currentPlayerIndex;
        var timer = playerInfoPanels[playerIndex].transform.Find("Timer");
        Image circle = timer.Find("Image").GetComponent<Image>();
        TextMeshProUGUI timerText = timer.Find("Image/TimerText").GetComponent<TextMeshProUGUI>();
        circle.color = last5 ? Color.red : Color.white;
        timerText.text = time.ToString();
        timerText.color = last5 ? Color.red : Color.white; 

    }

    public void SetTurnTimer()
    {
        int playerIndex = GameManager.Instance.currentPlayerIndex;
        for (var x = 0; x < playerInfoPanels.Count; x++)
        {
            if (x != playerIndex)
            {
                playerInfoPanels[x].transform.Find("Timer").gameObject.SetActive(false);
            }
            else
            {
                playerInfoPanels[x].transform.Find("Timer").gameObject.SetActive(true);
            }
        }
    }

    public void PassButton()
    {
        GameManager.Instance.CmdPassPurchase();
        HandleButtonStates(null);
    }

    // public void HandleButtonStates(TileRuntimeData? tile)
    // {
    //     PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();
    //     if (!currentPlayer.isLocalPlayer)
    //     {
    //         SetGroup(null);
    //         return;

    //     }
    //     if (currentPlayer.isMoving || !GameManager.Instance.turnManager.isLocked)
    //     {
    //         SetGroup(null);
    //         return;
    //     }
    //     if (!currentPlayer.hasRolledDice)
    //     {
    //         SetGroup(rollDiceGroup);
    //         return;
    //     }
    //     else
    //     {
    //         switch (tile?.tileData.tileType)
    //         {
    //             case TileType.Property:
    //                 PropertyData property = (PropertyData)tile.tileData;
    //                 if (tile.owner == currentPlayer)
    //                 {
    //                     SetGroup(buildGroup, tile.hasHouse, tile.hasHotel, property:property);
    //                 }
    //                 else
    //                 {
                        
    //                     SetGroup(propertyActionGroup, tile.hasHouse, tile.hasHotel,property:property);
    //                 }
    //                 break;
    //             case TileType.Utility:
    //             case TileType.Station:
    //                 UoSData uoSData = (UoSData)tile.tileData;
    //                 SetGroup(propertyActionGroup,uoS:uoSData);
    //                 break;
    //             case TileType.Chance:
    //             case TileType.Community:
    //                 SetGroup(rollDiceGroup);
    //                 break;
    //             case TileType.Tax:
    //             case TileType.GoToJail:
    //             case TileType.Corner:
    //                 SetGroup(rollDiceGroup);
    //                 break;
    //             default:
    //                 Debug.Log("[UI_MANAGER] HandleButtonStates received null");
    //                 SetGroup(null);
    //                 break;
    //         }
    //     }
    // }
    public void HandleButtonStates(TileRuntimeData? tile)
    {
        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();

        // Eğer sıra bende değilse veya yerel oyuncu değilsem UI'ı direkt kapat.
        if (!currentPlayer.isLocalPlayer)
        {
            SetGroup(null);
            return;
        }

        // Karakter hareket ediyorsa veya sistem bir işlem için kilitliyse buton gösterme.
        if (currentPlayer.isMoving || !GameManager.Instance.turnManager.isLocked)
        {
            SetGroup(null);
            return;
        }

        // ZAR ATILMADIYSA: Tek seçenek zar atmaktır.
        if (!currentPlayer.hasRolledDice)
        {
            SetGroup(rollDiceGroup);
            return; 
        }
        
        // ZAR ATILDIYSA AMA HENÜZ KARAR VERİLMEDİYSE:
        // tile null ise (boş geçilen kareler) veya karar zaten verildiyse UI'ı kapat.
        if (currentPlayer.hasMadeDecision || tile == null)
        {
            SetGroup(null);
            return;
        }
        // Buraya geldiğimizde biliyoruz ki: Sıra bizde, hareket bitti, zar atıldı ve karar verilmedi.
        switch (tile.tileData.tileType)
        {
            case TileType.Property:
                PropertyData property = (PropertyData)tile.tileData;
                
                if (tile.owner == currentPlayer)
                {
                    // Kendi mülkümüzse bina dikme panelini aç
                    SetGroup(buildGroup, tile.hasHouse, tile.hasHotel, property: property);
                }
                else if (tile.owner == null)
                {
                    // Sahipsizse satın alma panelini aç
                    SetGroup(propertyActionGroup, tile.hasHouse, tile.hasHotel, property: property);
                }
                else
                {
                    // Başkasınınsa kira ödeme
                    SetGroup(null); 
                }
                break;

            case TileType.Utility:
            case TileType.Station:
                UoSData uoSData = (UoSData)tile.tileData;
                if (tile.owner == null)
                    SetGroup(propertyActionGroup, uoS: uoSData);
                else
                    SetGroup(null);
                break;

            // Özel karelerde karar mekanizması yoksa (otomatikse) null dön.
            case TileType.Chance:
            case TileType.Community:
            case TileType.Tax:
            case TileType.GoToJail:
            case TileType.Corner:
                SetGroup(null); 
                break;

            default:
                SetGroup(null);
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
    public void RemovePlayerInfoPanel(PlayerScript playerToRemove)
    {
        foreach (var cardObj in playerInfoPanels)
        {
            PlayerCard card = cardObj.GetComponent<PlayerCard>();
            if (card.owner == playerToRemove)
            {
                cardObj.SetActive(false); // Kartı gizle
                // İstersen kartı listeden de silebilirsin ama SetActive(false) yeterli olur
                break; 
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        // if (index >= 0 && index < playerInfoPanels.Count)
        // {
        //     GameObject panelToRemove = playerInfoPanels[index];
        //     playerInfoPanels.RemoveAt(index);
        //     Destroy(panelToRemove);
        // }
    }
    public void SetWinnerUI(PlayerScript player)
    {
        var uiElements = GameManager.Instance.GetUIElements();
        var panel = uiElements.detailPanel.gameObject;
        panel.SetActive(true);
        var winnerPanel = panel.transform.Find("WinPanel").gameObject;
        winnerPanel.SetActive(true);
        var winnerCard = winnerPanel.transform.Find("WinnerCard").gameObject;
        var textComponent = winnerCard.transform.Find("WinnerName").GetComponent<TextMeshProUGUI>();
        RawImage avatarComponent = winnerCard.transform.Find("WinnerAvatar").GetComponent<RawImage>();
        textComponent.text = player.playerName;
        avatarComponent.texture = player.steamAvatarTexture;

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