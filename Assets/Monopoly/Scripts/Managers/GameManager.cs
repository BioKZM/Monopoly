#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Tilemaps;
using TMPro;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    #region Manager References
    [Header("Managers")]
    public TurnManager turnManager;
    public PropertyManager propertyManager;
    public UIManager uiManager;
    public CardManager cardManager;
    public BankruptcyManager bankruptcyManager;
    public EventManager eventManager;
    #endregion

    #region Game Data
    [Header("Game Data")]
    public List<TileData> tileDefinitions;
    public List<PlayerScript> players = new List<PlayerScript>();
    public List<GameObject> propertyTiles = new List<GameObject>();
    public List<CardData> chanceCardEffects;
    public List<CardData> communityCardEffects;
    #endregion

    #region UI Elements
    [Header("UI Elements")]
    public Button rollDiceButton;
    public Button buyButton;
    public Button buyHouseButton;
    public Button buyHotelButton;
    public Button passButton;
    public Button drawerButton;
    public CanvasGroup drawerGroup;
    public CanvasGroup rollDiceGroup;
    public CanvasGroup propertyActionGroup;
    public CanvasGroup detailPanel;
    public CanvasGroup buildGroup;
    public GameObject bankruptcyCard;
    public List<GameObject> playerInfoPanels = new List<GameObject>();
    public int handDeterminedDice_;
    public TMP_FontAsset messageFont;
    public int messageFontSize = 36;
    public RectTransform bannerRect;
    public int maxQueue = 99;
    #endregion

    #region Unity Lifecycle Methods
    void Awake()
    {
        InitializeSingleton();
        InitializeManagers();
    }

    void Start()
    {
        InitializeGame();
        uiManager.UpdateUI();
        turnManager.StartTurn();
    }

    void Update()
    {
        DebugGameState();
    }
    #endregion

    #region Initialization Methods
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void InitializeManagers()
    {
        // Manager bileşenlerini otomatik olarak bul ve ata
        if (turnManager == null) turnManager = GetComponent<TurnManager>();
        if (propertyManager == null) propertyManager = GetComponent<PropertyManager>();
        if (uiManager == null) uiManager = GetComponent<UIManager>();
        if (cardManager == null) cardManager = GetComponent<CardManager>();
        if (bankruptcyManager == null) bankruptcyManager = GetComponent<BankruptcyManager>();
        if (eventManager == null) eventManager = GetComponent<EventManager>();

        // Eğer manager bileşenleri yoksa, otomatik olarak ekle
        if (turnManager == null) turnManager = gameObject.AddComponent<TurnManager>();
        if (propertyManager == null) propertyManager = gameObject.AddComponent<PropertyManager>();
        if (uiManager == null) uiManager = gameObject.AddComponent<UIManager>();
        if (cardManager == null) cardManager = gameObject.AddComponent<CardManager>();
        if (bankruptcyManager == null) bankruptcyManager = gameObject.AddComponent<BankruptcyManager>();
        if (eventManager == null) eventManager = gameObject.AddComponent<EventManager>();
    }

    private void InitializeGame()
    {
        InitializePlayers();
        InitializeTiles();
        SetupManagers();
        uiManager.AddButtonListeners();
    }

    private void InitializePlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (var playerObject in playerObjects)
        {
            var playerScript = playerObject.GetComponent<PlayerScript>();
            players.Add(playerScript);
        }
    }

    private void InitializeTiles()
    {
        propertyManager.tileRuntimeList.Clear();
        foreach (var tile in tileDefinitions)
        {
            TileRuntimeData runtimeData = new TileRuntimeData
            {
                tileData = tile,
                owner = null,
                hasHouse = false,
                hasHotel = false,
            };
            propertyManager.tileRuntimeList.Add(runtimeData);
        }
    }

    private void SetupManagers()
    {
        // Manager'lara gerekli referansları ata
        turnManager.players = players;
        
        // propertyManager.tileRuntimeList = propertyManager.tileRuntimeList;
        propertyManager.propertyTiles = propertyTiles;
        
        uiManager.InitializeUI();
        
        cardManager.chanceCardEffects = chanceCardEffects;
        cardManager.communityCardEffects = communityCardEffects;
    }
    #endregion

    #region Public Methods
    public PlayerScript GetCurrentPlayer()
    {
        return turnManager.GetCurrentPlayer();
    }

    public List<PlayerScript> GetPlayers()
    {
        return players;
    }

    public TurnManager GetTurnManager()
    {
        return turnManager;
    }

    public PropertyManager GetPropertyManager()
    {
        return propertyManager;
    }

    public UIManager GetUIManager()
    {
        return uiManager;
    }

    public CardManager GetCardManager()
    {
        return cardManager;
    }

    public TileRuntimeData GetRuntimeTile(int index)
    {
        return propertyManager.GetRuntimeTile(index);
    }

    public bool HasFullColorSet(PlayerScript player, string colorGroup)
    {
        return propertyManager.HasFullColorSet(player, colorGroup);
    }

    public bool IsTilePurchasable(TileRuntimeData tile)
    {
        return propertyManager.IsTilePurchasable(tile);
    }

    public void UpdateUI()
    {
        uiManager.UpdateUI();
    }

    public void HandleButtonStates(TileRuntimeData? tileData)
    {
        uiManager.HandleButtonStates(tileData);
    }

    public void HandleChanceOrCommunityTile(TileRuntimeData currentTile)
    {
        cardManager.HandleChanceOrCommunityTile(currentTile);
    }
    public void SetupCardUI(string cardText, bool isChanceCard)
    {
        uiManager.SetupCardUI(cardText, isChanceCard);
    }
    public void InitiateBankruptcy(PlayerScript bankruptedPlayer)
    {
        bankruptcyManager.InitiateBankruptcy(bankruptedPlayer);
    }
    
    public UIElements GetUIElements()
    {
        return new UIElements
        {
            rollDiceButton = rollDiceButton,
            buyButton = buyButton,
            buyHouseButton = buyHouseButton,
            buyHotelButton = buyHotelButton,
            passButton = passButton,
            rollDiceGroup = rollDiceGroup,
            propertyActionGroup = propertyActionGroup,
            buildGroup = buildGroup,
            playerInfoPanels = playerInfoPanels,
            drawerGroup = drawerGroup,
            drawerButton = drawerButton,
            detailPanel = detailPanel,
            bankruptcyCard = bankruptcyCard

        };
    }
    public void SetGroup(CanvasGroup activeGroup)
    {
        uiManager.SetGroup(activeGroup);
    }
    public void ShowPropertyDetails(string tileName, TileRuntimeData tileData)
    {
        uiManager.ShowPropertyDetails(tileName,tileData);
    }

    public TileRuntimeData GetRuntimeTileByName(string tileName)
    {
        return propertyManager.GetRuntimeTileByName(tileName);
    }
    #endregion
    public Color GetTileColor(TileData tileData)
    {
        return propertyManager.GetTileColor(tileData);
    }

    public void ShowPurchase(PlayerScript buyer, TileRuntimeData tile)
    {
        eventManager.ShowPurchase(buyer, tile);
    }
    public void ShowSelling(PlayerScript seller, TileRuntimeData tile)
    {
        eventManager.ShowSelling(seller, tile);
    }

    public void ShowRentPayment(PlayerScript payer, PlayerScript payee, TileRuntimeData tile, int amount)
    {
        eventManager.ShowRentPayment(payer, payee, tile, amount);
    }

    public void ShowGoToJail(PlayerScript player)
    {
        eventManager.ShowGoToJail(player);
    }

    public void ShowBankrupt(PlayerScript player)
    {
        eventManager.ShowBankrupt(player);
    }
    public void ShowBuild(PlayerScript player, TileRuntimeData tile, string buildingName)
    {
        eventManager.ShowBuild(player, tile, buildingName);
    }
    public void ShowTaxPayment(PlayerScript player, TileRuntimeData tile, int amount)
    {
        eventManager.ShowTaxPayment(player, tile, amount);
    }

    public void ShowCustom(string template, PlayerScript? player = null, TileRuntimeData? tile = null, string? building = null, PlayerScript? player2 = null)
    {
        eventManager.ShowCustom(template, player, tile, building, player2);
    }
    





    public void RemovePlayerFromGame(PlayerScript player)
    {
        var index = players.FindIndex(p => p == player);
        players.Remove(player);
        Destroy(player.gameObject);
        uiManager.RemovePlayerInfoPanel(index);
        turnManager.EndTurn();
    }

    public void HandleWin(PlayerScript player)
    {
        Time.timeScale = 0;
        uiManager.SetWinnerUI(player.playerName);
        
    }

    #region Debug
    private void DebugGameState()
    {
        // Debug.Log($"Current Player: {GetCurrentPlayer().name} \n Jail Status: {GetCurrentPlayer().isInJail}");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetCurrentPlayer().money = -1000;
            foreach (var tile in propertyManager.tileRuntimeList)
            {
                Debug.Log(tile.tileData.tileName);
                Debug.Log(tile.owner);
                // Debug.Log(tile.houseCount);
                Debug.Log(tile.hasHotel);
            }
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Debug.Log(GetCurrentPlayer().money);
            Debug.Log(GetCurrentPlayer().name);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            propertyManager.GiveAllTilesToPlayer(GetCurrentPlayer());
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            GetCurrentPlayer().money -= 9999999;
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            turnManager.handDeterminedDice = handDeterminedDice_;
        }
    }
    #endregion
}
