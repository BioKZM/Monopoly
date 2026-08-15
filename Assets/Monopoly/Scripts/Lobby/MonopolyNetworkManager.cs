using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class MonopolyNetworkManager : NetworkRoomManager
{
    public struct PlayerSessionData
    {
        public int characterIndex;
        public Color playerColor;
        public string steamName;
        public ulong steamID;
        public int backgroundIndex;
    }
    public GameObject networkDataHelper;
    static private Dictionary<int, PlayerSessionData> lobbyDataCache = new();
    public string currentLobbyCode;
    public static MonopolyNetworkManager Instance => singleton as MonopolyNetworkManager;

    public override void Awake() 
    {
        if (singleton != null && singleton != this) {
            Destroy(gameObject);
            return;
        }
        base.Awake();
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
        GameObject dataHelper = Instantiate(networkDataHelper);
        NetworkServer.Spawn(dataHelper);
    }
    public override void OnStartHost()
    {
        base.OnStartHost();

        // Host başlar başlamaz Lobi sahnesine ZORLA geçiş yap
        if (!string.IsNullOrEmpty(RoomScene))
        {
            Debug.Log($"[FORCE] Host başladı, Sahne değişiyor: {RoomScene}");
            ServerChangeScene(RoomScene);
        }
    }
    
    // --- LOBİYE GİRİNCE OYUNCU YARATMA ---
    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();

        string currentScenePath = SceneManager.GetActiveScene().path;
        string currentSceneName = SceneManager.GetActiveScene().name;

        Debug.Log($"[SCENE DEBUG] Yüklenen: {currentScenePath} | Beklenen RoomScene: {RoomScene}");

        bool isLobby = currentScenePath == RoomScene || 
                       currentScenePath.Contains(RoomScene) || 
                       currentSceneName.Contains("Lobby") ||
                       (RoomScene != null && currentScenePath.Contains(RoomScene));

        if (isLobby) 
        {
            currentLobbyCode = SteamLobbyController.Instance.currentLobbyCode;
            UpdateLobbyCodeTextUI();
            EnsureLobbyPlayerListRenderer();

            // Eğer daha önce oyuncu yaratılmadıysa yarat
            if (NetworkClient.connection != null && NetworkClient.connection.identity == null)
            {
                Debug.Log("--- [SPAWN] Lobi doğrulandı, Oyuncu yaratılıyor (AddPlayer) ---");
                NetworkClient.AddPlayer();
            }
            else
            {
                Debug.Log("[SPAWN] Oyuncu zaten var, tekrar yaratılmadı.");
            }
        }
    }
    
    // --- OYUN SAHNESİ YÜKLENİNCE ÇALIŞIR ---
    public override void OnServerSceneChanged(string sceneName)
    {
        // 1. Önce Mirror'ın standart işini yapmasına izin ver
        base.OnServerSceneChanged(sceneName);

        Debug.Log($"[SCENE CHANGE] Yüklenen Sahne: {sceneName} | Beklenen Oyun Sahnesi: {GameplayScene}");

        // 2. KONTROL: Eğer Mirror piyonları doğurmadıysa ve şu an oyun sahnesindeysek
        bool isGameScene = sceneName == GameplayScene || 
                           sceneName.Contains("Game") ||
                           (GameplayScene != null && sceneName.Contains(GameplayScene));

        if (isGameScene)
        {
            StartCoroutine(SpawnPlayersWithData());
            
        }
    }
    private IEnumerator SpawnPlayersWithData()
    {
        int expectedPlayers = lobbyDataCache.Count;
        while (NetworkServer.connections.Count < expectedPlayers) 
        {
            yield return new WaitForSeconds(0.1f);
        }

        bool allReady = false;
        while (!allReady)
        {
            allReady = true;
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn == null || !conn.isReady)
                {
                    allReady = false;
                    break;
                }
            }
            if (!allReady) yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("[SERVER] Tüm bağlantılar hazır, piyonlar dağıtılıyor...");

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (lobbyDataCache.TryGetValue(conn.connectionId, out PlayerSessionData data))
            {
                Transform startPos = GetStartPosition();
                GameObject gamePlayer = Instantiate(playerPrefab, startPos.position, startPos.rotation);

                var visualScript = gamePlayer.GetComponent<MonopolyGamePlayer>();
                
                visualScript.characterIndex = data.characterIndex;
                visualScript.playerColor = data.playerColor;
                visualScript.steamID = data.steamID;
                visualScript.steamName = data.steamName;
                visualScript.backgroundIndex = data.backgroundIndex;

                Debug.Log($"{data.steamName} için piyon hazırlandı. Index: {data.characterIndex}, Renk: {data.playerColor}");
                
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, ReplacePlayerOptions.KeepAuthority);
            }
        }
    }
    

    [Server]
    public void StartGameManually()
    {
        if (NetworkDataHelper.Instance == null)
        {
            Debug.LogError("[START_GAME_MANUALLY] NetworkDataHelper bulunamadı.");
            return;
        }

        lobbyDataCache.Clear();
        NetworkDataHelper.Instance.finalLobbyPlayers.Clear();

        foreach (var player in roomSlots)
        {
            if (player != null)
            {
                var roomPlayer = player as MonopolyRoomPlayer;
                if (roomPlayer != null)
                {
                    
                    PlayerSessionData data = new PlayerSessionData
                    {
                        characterIndex = roomPlayer.characterIndex,
                        playerColor = roomPlayer.playerColor,
                        steamName = roomPlayer.playerName,
                        steamID = roomPlayer.playerSteamId,
                        backgroundIndex = roomPlayer.backgroundIndex,
                    };
                    lobbyDataCache[player.connectionToClient.connectionId] = data;
                    NetworkDataHelper.Instance.finalLobbyPlayers.Add(data);


                    roomPlayer.ServerForceReady();
                }
                Debug.Log($"[KONTROL] {player.name} Hazır mı? : {player.readyToBegin}");
            }
        }

        Debug.Log("[MANAGER] Oyun Manuel Olarak Başlatılıyor! Sahne: " + GameplayScene);

        ServerChangeScene(GameplayScene);
    }

    public void UpdateLobbyCodeTextUI()
    {
        GameObject lobbyCodeObject = GameObject.Find("LobbyCode");
        if (lobbyCodeObject == null) return;

        var lobbyCodeText = lobbyCodeObject.GetComponent<TextMeshProUGUI>();
        if (lobbyCodeText == null) return;

        lobbyCodeText.text = "Lobi Kodu: " + currentLobbyCode;
    }

    private void EnsureLobbyPlayerListRenderer()
    {
        GameObject lobbyContent = GameObject.Find("LobbyContent");
        if (lobbyContent == null)
        {
            Debug.LogWarning("[LOBBY_UI] LobbyContent bulunamadı, oyuncu listesi çizilemedi.");
            return;
        }

        if (!lobbyContent.TryGetComponent(out LobbyPlayerListRenderer renderer))
        {
            renderer = lobbyContent.AddComponent<LobbyPlayerListRenderer>();
        }

        renderer.QueueRedraw();
    }

    public void CopyLobbyCode()
    {
        GUIUtility.systemCopyBuffer = currentLobbyCode;
    }
}
