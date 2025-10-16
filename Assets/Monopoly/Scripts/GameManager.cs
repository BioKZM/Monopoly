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
    public CanvasGroup rollDiceGroup;
    public CanvasGroup propertyActionGroup;
    public CanvasGroup buildGroup;
    public List<GameObject> playerInfoPanels = new List<GameObject>();
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

        // Eğer manager bileşenleri yoksa, otomatik olarak ekle
        if (turnManager == null) turnManager = gameObject.AddComponent<TurnManager>();
        if (propertyManager == null) propertyManager = gameObject.AddComponent<PropertyManager>();
        if (uiManager == null) uiManager = gameObject.AddComponent<UIManager>();
        if (cardManager == null) cardManager = gameObject.AddComponent<CardManager>();
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

    // public void AddPropertyCardToUI(string tileName)
    // {
    //     uiManager.AddPropertyCardToUI(tileName);
    // }

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
        };
    }
    public void SetGroup(CanvasGroup activeGroup)
    {
        uiManager.SetGroup(activeGroup);
    }
    #endregion

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
    }
    #endregion
}
