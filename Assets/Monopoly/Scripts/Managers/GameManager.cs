#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Linq;
using System.Collections;
using System;
using Random = UnityEngine.Random;


public class GameManager : NetworkBehaviour
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
    public LogManager logManager;
    
    #endregion

    #region Game Data
    [Header("Game Data")]
    public List<TileData> tileDefinitions;
    public List<GameObject> propertyTiles = new List<GameObject>();
    public List<CardData> chanceCardEffects;
    public List<CardData> communityCardEffects;
    public SkinDatabase skinDatabase;

    #endregion

    #region UI Elements
    [Header("UI Elements")]
    public Button rollDiceButton;
    public Button buyButton;
    public Button buyHouseButton;
    public Button buyHotelButton;
    public Button passButton;
    public Button drawerButton;
    public Button buildPassButton;
    public Button startMatchButton;
    public CanvasGroup drawerGroup;
    public CanvasGroup rollDiceGroup;
    public CanvasGroup propertyActionGroup;
    public CanvasGroup detailPanel;
    public CanvasGroup buildGroup;
    public GameObject bankruptcyCard;
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;
    public Transform logContentParent;
    public ScrollRect logScrollRect;
    public GameObject logPanel; 
    public TextMeshProUGUI logEntryPrefab;
    public Button logPanelButton;
    public GameObject startGameButton;
    public GameObject escPanel;
    public GameObject ownershipTextPrefab;
    public List<GameObject> playerInfoPanels = new List<GameObject>();
    public int handDeterminedDice_;
    public TMP_FontAsset messageFont;
    public int messageFontSize = 36;
    public RectTransform bannerRect;
    public int maxQueue = 99;
    public DiceVisual dice1Object;
    public DiceVisual dice2Object;
    public Transform diceAreaTransform;
    public TextMeshProUGUI turnCountText;
    public GameObject cameraTopDownPoint;
    public Transform cameraStartingPoint;
    public GameObject console;
    public GameObject consoleInputField;
    public GameObject consoleContent;
    public GameObject tileAura;
    
    #endregion

    #region Networking
    [SyncVar(hook=nameof(OnTurnChanged))] public int currentPlayerIndex = 0;
    
    [SyncVar(hook=nameof(OnTurnCountChanged))] public int turnCount = 0;
    
    [SyncVar(hook= nameof(OnTimerChanged))] public float turnTimer = 30f;
    
    [SyncVar] public readonly SyncList<PlayerScript> players = new SyncList<PlayerScript>();

    [SyncVar] public bool isPaused = true;

    [SyncVar] public bool isGameStarted = false;
    #endregion




    #region Unity Lifecycle Methods
    void Awake()
    {
        InitializeSingleton();
        InitializeManagers();
    }


    void Update()
    {
        if (isServer && isGameStarted && !isPaused)
        {
            if (turnTimer > 0)
            {
                turnTimer -= Time.deltaTime;
                
                PlayerScript player = GetCurrentPlayer();

                if (turnTimer <= 20f && !player.hasRolledDice)
                {
                    // Oyuncu hala zar atmadıysa server otomatik atsın
                    // rollDiceButton.onClick.Invoke();
                    CmdSetPaused(true);
                    ServerForceDiceRoll(player);

                    // CmdSetPaused(true);
                    // GetUIElements().rollDiceButton.enabled = false;
                    // CmdRequestRoll();

                    // player.hasRolledDice = true;
                }
            }

            else
            {
                CmdPassPurchase();
                uiManager.HandleButtonStates(null);
                // turnTimer = 30f;
                // if (!isServer) return;
                // uiManager.PassButton();
                // PlayerScript player = GetCurrentPlayer();
                // if (player == null) return;
                // player.hasMadeDecision = true;
                // player.hasRolledDice = true;
                // ServerEndTurn();
            }
        }
        // DebugGameState();
    }
    #endregion

    #region Initialization Methods
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("[GameManager] Ben yok ediliyorum!");

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
        if (logManager == null) logManager = GetComponent<LogManager>();
        
        
        // Eğer manager bileşenleri yoksa, otomatik olarak ekle
        if (turnManager == null) turnManager = gameObject.AddComponent<TurnManager>();
        if (propertyManager == null) propertyManager = gameObject.AddComponent<PropertyManager>();
        if (uiManager == null) uiManager = gameObject.AddComponent<UIManager>();
        if (cardManager == null) cardManager = gameObject.AddComponent<CardManager>();
        if (bankruptcyManager == null) bankruptcyManager = gameObject.AddComponent<BankruptcyManager>();
        if (eventManager == null) eventManager = gameObject.AddComponent<EventManager>();
        if (logManager == null) logManager = gameObject.AddComponent<LogManager>();
        


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
        turnManager.players = this.players;
        
        propertyManager.propertyTiles = propertyTiles;
        
        uiManager.InitializeUI();
        
        cardManager.chanceCardEffects = chanceCardEffects;
        cardManager.communityCardEffects = communityCardEffects;
        logManager.SetupLogManager(logContentParent,logScrollRect,logPanel,logPanelButton,logEntryPrefab);
        
    }
    #endregion

    #region Public Methods
    public PlayerScript GetCurrentPlayer()
    {
        return turnManager.GetCurrentPlayer();
    }

    public TurnManager GetTurnManager()
    {
        return turnManager;
    }

    public PropertyManager GetPropertyManager()
    {
        return propertyManager;
    }



    public CardSkinData GetSkinByID(int skinID)
    {
        return skinDatabase.GetSkinByID(skinID);
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
            bankruptcyCard = bankruptcyCard,
            buildPassButton = buildPassButton,
            ownershipTextPrefab = ownershipTextPrefab,
            loadingPanel = loadingPanel,
            loadingText = loadingText,
            startGameButton = startGameButton,
            escPanel = escPanel,
            console = console,
            consoleInputField = consoleInputField,
            consoleContent = consoleContent,


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

    public Color GetTileColor(TileData tileData)
    {
        return propertyManager.GetTileColor(tileData);
    }

    #endregion


    #region Events

    public void ShowPurchase(int buyerIndex, string tileName)
    {
        PlayerScript buyer = players[buyerIndex];
        var tile = propertyManager.GetRuntimeTileByName(tileName);
        eventManager.ShowPurchase(buyer,tile);
    }
    public void ShowDice(int dice1, int dice2)
    {
        PlayerScript player = turnManager.currentPlayer;
        eventManager.ShowDice(player,dice1, dice2);
    }


    [ClientRpc]
    public void RpcShowSelling(int sellerIndex, string tileName)
    {
        PlayerScript seller = players[sellerIndex];
        var tile = propertyManager.GetRuntimeTileByName(tileName);
        eventManager.ShowSelling(seller, tile);
    }

    [ClientRpc]
    public void RpcShowRentPayment(int payerIndex, int payeeIndex, string tileName, int amount)
    {
        PlayerScript payer = players[payerIndex];
        PlayerScript payee = players[payeeIndex];
        var tile = propertyManager.GetRuntimeTileByName(tileName);
        eventManager.ShowRentPayment(payer, payee, tile, amount);
    }

    [ClientRpc]
    public void RpcShowGoToJail(int playerIndex)
    {
        PlayerScript player = players[playerIndex];
        eventManager.ShowGoToJail(player);
    }

    [ClientRpc]
    public void RpcShowBankrupt(int playerIndex)
    {
        PlayerScript player = players[playerIndex];
        eventManager.ShowBankrupt(player);
    }

    // [ClientRpc]
    public void ShowBuild(int playerIndex, string tileName, string buildingName)
    {
        PlayerScript player = players[playerIndex];
        var tile = propertyManager.GetRuntimeTileByName(tileName);
        eventManager.ShowBuild(player, tile, buildingName);
    }

    [ClientRpc]
    public void RpcShowTaxPayment(int playerIndex, string tileName, int amount)
    {
        PlayerScript player = players[playerIndex];
        var tile = propertyManager.GetRuntimeTileByName(tileName);
        eventManager.ShowTaxPayment(player, tile, amount);
    }

    [ClientRpc]
    public void RpcShowBailPayment(int playerIndex, int amount)
    {
        PlayerScript player = players[playerIndex];
        eventManager.ShowBailPayment(player, amount);
    }

    [ClientRpc]
    public void RpcShowTimeUp(int playerIndex)
    {
        PlayerScript player = players[playerIndex];
        eventManager.ShowTimeUp(player);
    }


    #endregion


    public List<TileRuntimeData> GetPlayerOwnedTiles(PlayerScript player)
    {
        return propertyManager.GetPlayerOwnedTiles(player);
    }
    public void SetOwnershipPanel(int playerIndex)
    {
        uiManager.ShowUserTiles(playerIndex);
    }

    public Color GetTextColor(TileRuntimeData tile)
    {
        return uiManager.GetTextColor(tile);
    }

    public void HandleWin(PlayerScript player)
    {
        Time.timeScale = 0;
        uiManager.SetWinnerUI(player);
    }





    #region F#CK1NG NETWORKING
    [SyncVar (hook = nameof(OnReadyCountChanged))] public int playersReportedReady = 0;
    [SyncVar] public int expectedPlayerCount = 0;
    public void InitializeGameLocal()
    {
        Debug.Log($"[DEBUG] InitializeGameLocal çağrıldı. Obje: {gameObject.name}, Aktif mi: {gameObject.activeInHierarchy}");
        if(!gameObject.activeInHierarchy) 
        {
            Debug.LogError("[DEBUG] GameManager aktif değil.");
            return; 
        }
        // 1. UIManager'ı ayağa kaldır (Referansları bağla)
        uiManager.InitializeUI();

        // 2. Lobiden gelen kesin oyuncu sayısını al
        // MonopolyNetworkManager içindeki o mermi gibi statik listeyi kullanıyoruz
        // var lobbyPlayers = MonopolyNetworkManager.finalLobbyPlayers;
        // int playerCount = lobbyPlayers.Count;

        // 3. Slotları (Check-In) lobideki sayıya göre oluştur
        // Bu sayede "kim bağlandı" yarışı bitiyor, slotlar baştan belli
        // uiManager.CreateCheckInSlots(playerCount); 

        // 4. Manager'ları ve oyun alanını kur
        InitializeTiles();

        SetupManagers();
        bankruptcyManager.InitalizeUI();


        // 5. Host isen butonu hazırla
        if (startMatchButton != null)
        {
            
            if (isServer)
            {
                startMatchButton.onClick.RemoveAllListeners();
                startMatchButton.onClick.AddListener(OnStartButtonClick);
                startMatchButton.interactable = false; // Herkes dolana kadar kapalı
            }
            else
            {
                startMatchButton.gameObject.SetActive(false);
            }
        }

        
        // 6. UI'ı AddButtonListeners ile butonlara bağla
        uiManager.AddButtonListeners();

        // 7. İllüzyonu başlat: Siyah ekranın arkasında verileri kontrol etmeye başla
        // Not: Bu korutin PlayerScript OnStartLocalPlayer'da da tetiklenebilir 
        // ama burada genel kurulum bittiği için en güvenli yer burasıdır.
        StartCoroutine(VerifyAndStartIllusion());
    }


    public IEnumerator VerifyAndStartIllusion()
    {
        
        bool isEverythingReady = false;
        int expectedCount = NetworkDataHelper.Instance.finalLobbyPlayers.Count;

        while (!isEverythingReady)
        {
            
            // 1. Önce piyonların (PlayerScript) listeye eklenip eklenmediğini kontrol et
            Debug.Log($"[Illusion] - players.Count = {players.Count} & expectedCount = {expectedCount}");
            if (players.Count >= expectedCount && expectedCount > 0)
            {
                
                // 3. Verilerin içeriğini check et
                bool allDataSettled = true;
                foreach (var p in players)
                {
                    Debug.Log($"[VERIFY] playerName: {p.playerName}, playerColor: {p.playerColor}, isAvatarNull: {p.steamAvatarTexture == null}");
                    // İsim Loading mi? Renk beyaz mı? Avatar gelmiş mi?
                    if (string.IsNullOrEmpty(p.playerName) || 
                        p.playerName == "Loading..." || 
                        p.playerColor == Color.white || 
                        p.steamAvatarTexture == null)
                    {
                        allDataSettled = false;
                        break;
                    }
                }
                
                // 2. Senin canavar fonksiyonunla UI'ı güncellemeye zorla
                uiManager.SetPlayersInfo(players);

                // Debug.Log($"[VERIFY] allDataSettled: {allDataSettled}");

                // 4. Client kendi içinde "Okeyim" dedi, Server'a rapor ver
                if (allDataSettled)
                {
                    isEverythingReady = true;
                    if (NetworkClient.localPlayer)
                    {
                        var localScript = NetworkClient.localPlayer.GetComponent<PlayerScript>();
                        Debug.Log($"[ILLUSION] Player ready, netID: {NetworkClient.localPlayer.netId}");
                        // localScript.FindTiles();
                        localScript.CmdSetReadyStatus(true);
                    }
                    // if (isEverythingReady) yield break;
                } 
            }
            
            // Veriler oturana kadar siyah ekran arkasında 0.2 saniyede bir döner
            yield return new WaitForSeconds(0.2f);
            // Debug.Log($"[VERIFY] isEverythingReady: {isEverythingReady}");
        }

        
    }

    [ClientRpc]
    public void RpcBeginMatch() {
        
        if (uiManager.loadingPanel != null) 
        {
            uiManager.loadingPanel.SetActive(false); 
        }
        StartCoroutine(WaitForPlayersAndStartTurn(0));
    }



    

    [Command(requiresAuthority = false)]
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public void CmdReportReady(NetworkConnectionToClient sender = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    {
        // Yarış durumunu engellemek için sayı 0 ise lobiden tekrar çek
        if (expectedPlayerCount <= 0)
            expectedPlayerCount = NetworkManager.singleton.numPlayers;

        playersReportedReady++;
        Debug.Log($"[SERVER] Hazır raporu geldi: {playersReportedReady} / {expectedPlayerCount}");

        // Herkes hazırsa Host'un butonunu yak (Veya direkt oyunu başlat)
        if (playersReportedReady >= expectedPlayerCount && expectedPlayerCount > 0)
        {
            EnableStartButtonOnHost(true);
        }
    }
    void OnReadyCountChanged(int oldVal, int newVal)
    {
        // UIManager'da loading yazısını güncelle: "Oyuncular Hazırlanıyor... (2/4)" gibi
        if (uiManager != null && uiManager.loadingText != null)
        {
            uiManager.UpdateLoadingStatus(newVal,expectedPlayerCount);
        }
    }
    public void EnableStartButtonOnHost(bool status)
    {
        // Bu metod sadece Host (Server) olan oyuncunun ekranında çalışır
        if (startMatchButton != null)
        {
            startMatchButton.interactable = status;
           
        }
    }
    public void OnStartButtonClick()
    {
        
        // Sadece Host/Server bu butona basabilir
        if (isServer)
        {
            isGameStarted = true;
            RpcBeginMatch();
            turnCount++;
        }
    }

    [Command(requiresAuthority = false)]
    #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public void CmdRequestRoll(NetworkConnectionToClient sender = null)
    #pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    {
        Debug.Log("[GAME_MANAGER] - Roll requested from server with [command]");
        PlayerScript player = sender.identity.GetComponent<PlayerScript>();
        if (player != turnManager.GetCurrentPlayer())
        {
            Debug.Log("[GAME_MANAGER] - Player is not current player");
        }


        int d1 = Random.Range(1, 7);
        int d2 = Random.Range(1, 7);
        RpcSpawnAndRoll(player,d1,d2);

    }



    [Server]
    public void ServerForceDiceRoll(PlayerScript player)
    {
        
        int d1 = Random.Range(1, 7);
        int d2 = Random.Range(1, 7);
        RpcSpawnAndRoll(player,d1,d2);
    }


    [ClientRpc]
    public void RpcSpawnAndRoll(PlayerScript player, int d1, int d2)
    {
        Debug.Log("[GAME_MANAGER] - Roll requested with [clientRpc]");
        player.hasRolledDice = true;

        dice1Object.isStopped = false;
        dice2Object.isStopped = false;
        Vector3 spawnPoint1 = diceAreaTransform.position + new Vector3(-1, 1, 0);
        Vector3 spawnPoint2 = diceAreaTransform.position + new Vector3(1, 1, 0);
        dice1Object.Roll(d1,spawnPoint1);
        dice2Object.Roll(d2,spawnPoint2);

        StartCoroutine(CameraAnimation(player, d1, d2));
    }
    private IEnumerator CameraAnimation(PlayerScript player, int d1, int d2)
    {

        // var cam = Camera.main;
        // var cameraLock = cam.GetComponent<CameraPlayerLock>();
        // if (cameraLock != null) cameraLock.enabled = false; // Script'i komple kapatmak daha güvenli

        // Vector3 startPos = cam.transform.position;
        // Quaternion startRot = cam.transform.rotation;
        // float startSize = cam.orthographicSize;

        // // Hedef rotasyonu döngü DIŞINDA bir kez hesaplayalım (veya tahmini bir açı verelim)
        // Vector3 zoomPos = diceAreaTransform.position + new Vector3(0, 8, -6); 
        // Quaternion targetRot = Quaternion.LookRotation(diceAreaTransform.position - zoomPos);

        // Kamerayı zarlara kilitle
        turnManager.SetCameraLock(diceAreaTransform);


        // Zarların durmasını bekle
        yield return new WaitUntil(() => dice1Object.isStopped && dice2Object.isStopped);
        
        // Zarlar durduktan sonra kısa bir süzülme süresi (Opsiyonel)
        yield return new WaitForSeconds(0.5f);

        // Zar sonuçlarını event olarak göster.
        ShowDice(d1, d2);

        // Kamerayı tekrardan oyuncuya kitle
        turnManager.SetCameraLock(player.transform);

        // Eğer kamera zaten bir oyuncuya kitliyse, diğer oyuncuların kameralarını da güncelle
        foreach (var p in turnManager.players)
        {
            if (!p.isCameraTopDown)
            {
                turnManager.SetCameraLock(player.transform);
            }
        }

        // Hem arsa alıp hem hareket edemesin diye diğer işlemler kilitli.
        turnManager.isLocked = true;

        // Bu bloğu herkeste çalıştırmak yerine sadece server tarafında işlem yaptırıp daha sonrasında bunu diğer client'lara senkronize ediyoruz.
        if (NetworkServer.active)
        {
            if (player.isInJail)
            {
                turnManager.ExecuteJailLogic(player, d1, d2);
            }
            else
            {
                // Hareketi başlatmadan önce kameranın piyonu tam yakalaması için yarım salise bekle
                yield return new WaitForSeconds(0.1f);
                RpcBroadcastMovement(d1 + d2, d1 , d2);
            }
        }
    }
    
    
    [ClientRpc]
    public void RpcBroadcastMovement(int diceTotal, int die1, int die2)
    {
        StartCoroutine(turnManager.PlayerTurnCoroutine(diceTotal, die1, die2));
    }

    [Server]
    public void ServerEndTurn()
    {
        if (CheckWinConditions()) return;
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        if (players[currentPlayerIndex].bankrupted)
        {
            
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            
        }
        if (currentPlayerIndex == 0) turnCount++;
        turnTimer = 30f;
        isPaused = false;
        turnManager.isLocked = false;
    }

    void OnTurnChanged(int oldIndex, int newIndex)
    {
        StartCoroutine(WaitForPlayersAndStartTurn(newIndex));

    }
    void OnTurnCountChanged(int oldValue, int newValue)
    {
        Color randomTurnColor = Color.HSVToRGB(Random.value, 0.7f, 0.9f);
        string hexColor = ColorUtility.ToHtmlStringRGB(randomTurnColor);
        LogManager.Instance.AddLog($"\n\n<color=#{hexColor}><b>--- Tur {newValue} --- </b></color>\n\n");
    }
    void OnTimerChanged(float oldValue, float newValue)
    {
        int seconds = Mathf.CeilToInt(newValue);
        uiManager.UpdateTimer(seconds, last5:false);
        if (seconds < 5f)
        {
            uiManager.UpdateTimer(seconds, last5:true);
        }
    }

    IEnumerator WaitForPlayersAndStartTurn(int targetIndex)
    {
        while (turnManager.players == null || turnManager.players.Count <= targetIndex || turnManager.players[targetIndex] == null)
        {
            yield return null;
        }
        turnManager.currentPlayer = players[targetIndex];
        turnManager.UpdateUI();
        turnManager.StartTurn();
    }


    public bool CheckWinConditions()
    {
        PlayerScript? winner = null;

        if (players.Count(p => !p.bankrupted) == 1)
        {
            winner = players.Find(p => !p.bankrupted);  
        }
        else
        {
            foreach (var player in players)
            {
                if (turnManager.CheckColorSetWin(player) || turnManager.CheckRowWin(player))
                {
                    winner = player;
                }
            }
        }
        if (winner != null)
        {
            RpcShowWinner(winner);
            return true;
        }
        return false;
        
    }

    [ClientRpc]
    public void RpcShowWinner(PlayerScript winner)
    {
        HandleWin(winner);
    }


    [TargetRpc]
    public void TargetFailedJailRoll(NetworkConnection target, int d1, int d2)
    {
        GetUIElements().rollDiceButton.enabled = true;
    }

    [Command (requiresAuthority = false)]
    #pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    
    public void CmdProcessPurchase(int buildings, int tileIndex, NetworkConnectionToClient sender = null)
    
    #pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    {
        PlayerScript player = sender.identity.GetComponent<PlayerScript>();
        if (player != turnManager.GetCurrentPlayer()) return;
        TileRuntimeData currentTile = GetRuntimeTile(tileIndex);
        
        if (currentTile.tileData is PropertyData property)
        {
            int totalCost = property.price;
            if (buildings == 1) totalCost += property.houseCost;
            if (buildings == 2) totalCost += property.hotelCost;
            if (player.money >= totalCost)
            {
                player.money -= totalCost;

                if (!player.ownedTiles.Contains(currentTile.tileData.tileName))
                {
                    player.ownedTiles.Add(currentTile.tileData.tileName);
                }
                RpcOnPurchaseSuccess(player.netId, tileIndex, buildings);
            }
        }
        else if (currentTile.tileData is UoSData uos)
        {
            if (player.money >= uos.price)
            {
                player.money -= uos.price;
                RpcOnPurchaseSuccess(player.netId, tileIndex, -1);
            }
        }
        player.hasMadeDecision = true;
        

    }
    
    [ClientRpc]
    public void RpcOnPurchaseSuccess(uint playerNetID, int tileIndex, int buildings)
    {
        // 1. Oyuncuyu ağ kimliğinden bul
        if (NetworkClient.spawned.TryGetValue(playerNetID, out NetworkIdentity identity))
        {
            PlayerScript buyer = identity.GetComponent<PlayerScript>();
            int buyerIndex = players.IndexOf(buyer);
            TileRuntimeData currentTile = propertyManager.GetRuntimeTile(tileIndex);

            // 2. VERİ GÜNCELLEME: Her client kendi yerel listesini günceller
            currentTile.owner = buyer;
            currentTile.hasHouse = buildings == 1;
            currentTile.hasHotel = buildings == 2;
            

            // 3. GÖRSEL GÜNCELLEME
            switch (buildings)
            {
                case 0:
                    propertyManager.PlaceBuildings(propertyManager.propertyTiles[tileIndex],0,buyer.playerMaterial);
                    ShowPurchase(buyerIndex, currentTile.tileData.tileName);
                    break;
                case 1:
                    propertyManager.PlaceBuildings(propertyManager.propertyTiles[tileIndex], 1,buyer.playerMaterial);
                    ShowBuild(buyerIndex, currentTile.tileData.tileName, "Ev");
                    break;
                case 2:
                    propertyManager.PlaceBuildings(propertyManager.propertyTiles[tileIndex], 2,buyer.playerMaterial, buyer.playerMaterialDark);
                    ShowBuild(buyerIndex, currentTile.tileData.tileName, "Otel");
                    break;
                default:
                    ShowPurchase(buyerIndex, currentTile.tileData.tileName);
                    break;
                    
            }
            
            
        
        }
    }

    [Command(requiresAuthority = false)]
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public void CmdPassPurchase(NetworkConnectionToClient sender = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    {
        PlayerScript player = sender.identity.GetComponent<PlayerScript>();
        if (player != turnManager.GetCurrentPlayer()) return;
        player.hasMadeDecision = true;
    }



    [Command(requiresAuthority = false)]
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public void CmdRequestCard(int tileIndex, NetworkConnectionToClient sender = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    {
        PlayerScript targetPlayer = sender.identity.GetComponent<PlayerScript>();

        bool isChance = GetRuntimeTile(tileIndex).tileData.tileType == TileType.Chance;
        int randomIndex = Random.Range(0, isChance ? chanceCardEffects.Count : communityCardEffects.Count);
        CardData card = isChance ? chanceCardEffects[randomIndex] : communityCardEffects[randomIndex];
        card.Execute(targetPlayer);
        Debug.Log($"[COC_CARD] isChance : {isChance}, cardIndex : {randomIndex}, cardText: {isChance : cardManager.chanceData[randomIndex] ? cardManager.communityData[randomIndex]}, targetPlayer: {targetPlayer.name}");
        RpcShowCardEffect(randomIndex, isChance);
        
    }

    [ClientRpc]
    public void RpcShowCardEffect(int index, bool isChance)
    {
        string cardText = isChance ? cardManager.chanceData[index] : cardManager.communityData[index];
        SetupCardUI(cardText, isChance);
    }


    [Command(requiresAuthority = false)]
    public void CmdSetPaused(bool state)
    {
        isPaused = state;
        Debug.Log($"[GAME_MANAGER] Oyun duraklatma durumu [Command] ile değişti {state}");
    }


    #endregion

    public IEnumerator SmoothCameraMove(Vector3 targetPos, Quaternion targetRot, float targetSize, float duration)
    {
        var cam = Camera.main;
        var cameraLock = cam.GetComponent<CameraPlayerLock>();
        if (cameraLock != null) cameraLock.enabled = false; // Script'i komple kapatmak daha güvenli
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        float startSize = cam.orthographicSize;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            cam.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, smoothT);
            
            // Eğer zarlar gibi fizik objelerini takip ediyorsan WaitForFixedUpdate, 
            // genel UI/Board geçişleri için null kullanabilirsin.
            yield return null; 
        }
    }
    public IEnumerator SmoothCameraMove(bool resetCamera)
    {
        if (resetCamera)
        {
            var cam = Camera.main;
            var cameraLock = cam.GetComponent<CameraPlayerLock>();
            if (cameraLock != null) cameraLock.enabled = false; // Script'i komple kapatmak daha güvenli
            Vector3 startPos = cam.transform.position;
            Quaternion startRot = cam.transform.rotation;
            float startSize = cam.orthographicSize;
            
            var targetPos = players[currentPlayerIndex].gameObject.transform.position;
            var targetRot = players[currentPlayerIndex].gameObject.transform.rotation;
            // float targetSize = cameraStartingPoint.orthographicSize;

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / 1;
                float smoothT = Mathf.SmoothStep(0, 1, t);

                cam.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
                cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
                cam.orthographicSize = Mathf.Lerp(startSize, 25, smoothT);
                
                // Eğer zarlar gibi fizik objelerini takip ediyorsan WaitForFixedUpdate, 
                // genel UI/Board geçişleri için null kullanabilirsin.
                yield return null; 
            }
        }
        
    }





#region In-Game Console

    [Command(requiresAuthority = false)]
    public void CmdExecuteDebugCommand(string fullInput, NetworkConnectionToClient sender = null)
    {
        
        string[] args = fullInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0) return;
        string command = args[0].ToLower();

        switch (command)
        {
            case "/setmoney":
                ExecuteSetMoney(sender,args);
                break;
            case "/jail":
                ExecuteJail(sender,args);
                break;
            case "/tp":
                ExecuteTeleport(sender,args);
                break;
            case "/bankrupt":
                ExecuteBankrupt(sender,args);
                break;
            case "/rollemptydice":
                ExecuteEmptyDice(sender,args);
                break;
            default:
                string consoleReply = $"<color=#cc0000>Bilinmeyen komut: {command} </color>";
                TargetReplyConsole(sender,consoleReply);
                break;
        }
    }

    [TargetRpc]
    public void TargetReplyConsole(NetworkConnectionToClient? sender, string message)
    {
        uiManager.AppendToConsole(message);
    }

    public void ExecuteSetMoney(NetworkConnectionToClient? sender, string[] args)
    {
        if (args.Length < 3) return;
        
        if (!int.TryParse(args[1], out int playerIndex) || !int.TryParse(args[2], out int amount))
        {
            TargetReplyConsole(sender, "Geçersiz argümanlar. Kullanım: /setmoney [playerIndex] [amount]");
            return;
        }

        if (playerIndex > 0 && playerIndex <= players.Count)
        {
            PlayerScript targetPlayer = players[playerIndex];
            targetPlayer.money = amount;
            TargetReplyConsole(sender, $"{targetPlayer.playerName} adlı oyuncunun parası {amount} olarak ayarlandı.");
        }

        else
        {
            TargetReplyConsole(sender, "Geçersiz oyuncu indexi. Oyuncu sayısı: " + players.Count);
        }

    }

    public void ExecuteJail(NetworkConnectionToClient? sender, string[] args)
    {
        if (args.Length < 2) return;
        if (!int.TryParse(args[1], out int playerIndex))
        {
            TargetReplyConsole(sender, "Geçersiz argümanlar. Kullanım: /jail [playerIndex]");
            return;
        }
        if (playerIndex > 0 && playerIndex <= players.Count)
        {
            PlayerScript targetPlayer = players[playerIndex];
            targetPlayer.GoToJail();
            TargetReplyConsole(sender, $"{targetPlayer.playerName} adlı oyuncu hapse atıldı.");
        }
        else
        {
            TargetReplyConsole(sender, "Geçersiz oyuncu indexi. Oyuncu sayısı: " + players.Count);
        }
    }

    public void ExecuteTeleport(NetworkConnectionToClient? sender, string[] args)
    {

        if (args.Length < 3) return;
        if (!int.TryParse(args[1], out int playerIndex) || !int.TryParse(args[2], out int tileIndex))
        {
            TargetReplyConsole(sender, "Geçersiz argümanlar. Kullanım: /tp [playerIndex] [tileIndex]");
            return;
        }
        if (playerIndex > 0 && playerIndex <= players.Count)
        {
            PlayerScript targetPlayer = players[playerIndex];
            targetPlayer.RpcTeleportPlayer(tileIndex);
            TargetReplyConsole(sender, $"{targetPlayer.playerName} adlı oyuncu {GetRuntimeTile(tileIndex).tileData.tileName} konumuna ışınlandı.");
        }
        else
        {
            TargetReplyConsole(sender, "Geçersiz oyuncu indexi. Oyuncu sayısı: " + players.Count);
        }
    }
    public void ExecuteBankrupt(NetworkConnectionToClient? sender, string[] args)
    {
        if (args.Length < 2) return;
        if (!int.TryParse(args[1], out int playerIndex))
        {
            TargetReplyConsole(sender, "Geçersiz argümanlar. Kullanım: /bankrupt [playerIndex]");
            return;
        }
        if (playerIndex > 0 && playerIndex <= players.Count)
        {
            PlayerScript targetPlayer = players[playerIndex];
            InitiateBankruptcy(targetPlayer);
            TargetReplyConsole(sender, $"{targetPlayer.playerName} adlı oyuncu iflas etti.");
        }
        else
        {
            TargetReplyConsole(sender, "Geçersiz oyuncu indexi. Oyuncu sayısı: " + players.Count);
        }
    }

    public void ExecuteEmptyDice(NetworkConnectionToClient? sender, string[] args)
    {
        if (args.Length < 2) return;
        if (!int.TryParse(args[1], out int playerIndex))
        {
            TargetReplyConsole(sender, "Geçersiz argümanlar. Kullanım: /rollemptydice [playerIndex]");
            return;
        }
        if (playerIndex > 0 && playerIndex <= players.Count)
        {
            PlayerScript targetPlayer = players[playerIndex];
            targetPlayer.hasRolledDice = true;
            RpcBroadcastMovement(0, 0, 0);
            // RpcSpawnAndRoll(targetPlayer, 1, 1);
            TargetReplyConsole(sender, $"{targetPlayer.playerName} adlı oyuncu boş zar attı.");
        }
        else
        {
            TargetReplyConsole(sender, "Geçersiz oyuncu indexi. Oyuncu sayısı: " + players.Count);
        }
    }
    
    
#endregion



}


