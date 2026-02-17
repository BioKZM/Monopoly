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
    }
    public GameObject networkDataHelper;
    static private Dictionary<int, PlayerSessionData> lobbyDataCache = new();
    public string currentLobbyCode;
    public static MonopolyNetworkManager Instance => singleton as MonopolyNetworkManager;

    public override void Awake() 
    {
        if (NetworkManager.singleton != null && NetworkManager.singleton != this) {
            Destroy(gameObject); // Eğer zaten bir tane varsa, beni (yeni geleni) yok et
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

        // KONTROL: Sahne yolu RoomScene'i içeriyor mu VEYA Sahne adı "LobbyScene" mi?
        // Bu "OR" (||) operatörü sayesinde hata payını sıfıra indiriyoruz.
        bool isLobby = currentScenePath == RoomScene || 
                       currentScenePath.Contains(RoomScene) || 
                       currentSceneName.Contains("Lobby") || // En garanti yöntem
                       (RoomScene != null && currentScenePath.Contains(RoomScene));

        if (isLobby) 
        {
            currentLobbyCode = SteamLobbyController.Instance.currentLobbyCode;
            UpdateLobbyCodeTextUI();

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

        // 2. KONTROL: Eğer Mirror piyonları doğurmadıysa ve şu an Oyun Sahnesindeysek...
        // Not: Path/Name uyuşmazlığını önlemek için "Contains" kullanıyoruz.
        bool isGameScene = sceneName == GameplayScene || 
                           sceneName.Contains("Game") || // Sahne adında "Game" geçiyorsa (Senin sahne adına göre düzenle)
                           (GameplayScene != null && sceneName.Contains(GameplayScene));

        if (isGameScene)
        {
            StartCoroutine(SpawnPlayersWithData());
            
        }
    }
    private IEnumerator SpawnPlayersWithData()
    {
        // 1. GÜVENLİK: Tüm bağlantıların (Host + Clientlar) sahneye girdiğinden emin ol
        int expectedPlayers = lobbyDataCache.Count;
        
        // Herkes 'isReady' olana kadar bekle (Sonsuz döngü olmasın diye max süre koyabilirsin)
        while (NetworkServer.connections.Count < expectedPlayers) 
        {
            yield return new WaitForSeconds(0.1f);
        }

        // 2. Herkesin bağlantısı geldi, şimdi 'isReady' olmalarını bekle
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

        // 3. ŞİMDİ piyonları yarat ve verileri bas
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (lobbyDataCache.TryGetValue(conn.connectionId, out PlayerSessionData data))
            {
                Transform startPos = GetStartPosition();
                GameObject gamePlayer = Instantiate(playerPrefab, startPos.position, startPos.rotation);

                var visualScript = gamePlayer.GetComponent<MonopolyGamePlayer>();
                
                // Verileri bas (SyncVar olduklarından emin ol!)
                visualScript.characterIndex = data.characterIndex;
                visualScript.playerColor = data.playerColor;
                visualScript.steamID = data.steamID;
                visualScript.steamName = data.steamName;

                Debug.Log($"[ABİN GELDİ YARRAM] {data.steamName} için piyon hazırlandı. Index: {data.characterIndex}, Renk: {data.playerColor}");
                
                // Client'ın piyonunu değiştir
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, ReplacePlayerOptions.KeepAuthority);
            }
        }
    }
    

    [Server]
    public void StartGameManually()
    {
        if (NetworkDataHelper.Instance == null)
        {
            Debug.LogError("[START_GAME_MANUALLY]NetworkDataHelper bulunamadı.");
            return;
        }

        lobbyDataCache.Clear();
        NetworkDataHelper.Instance.finalLobbyPlayers.Clear();
        // 1. GÜVENLİK: Odadaki herkesi ZORLA 'Hazır' yap
        // Böylece Mirror "Bu adam hazır değildi" diyip piyonu unutmamazlık yapamaz.
        foreach (var player in roomSlots)
        {
            if (player != null)
            {
                var roomPlayer = player as MonopolyRoomPlayer;
                if (roomPlayer != null)
                {
                    // lobbyDataCache[player.connectionToClient.connectionId] = new PlayerSessionData
                    PlayerSessionData data = new PlayerSessionData
                    {
                        characterIndex = roomPlayer.characterIndex,
                        playerColor = roomPlayer.playerColor,
                        steamName = roomPlayer.playerName,
                        steamID = roomPlayer.playerSteamId
                    };
                    lobbyDataCache[player.connectionToClient.connectionId] = data;
                    NetworkDataHelper.Instance.finalLobbyPlayers.Add(data);


                    roomPlayer.ServerForceReady();
                }
                Debug.Log($"[KONTROL] {player.name} Hazır mı? : {player.readyToBegin}");
            }
        }

        Debug.Log("[MANAGER] Oyun Manuel Olarak Başlatılıyor! Sahne: " + GameplayScene);

        // 2. Sahneyi Değiştir
        ServerChangeScene(GameplayScene);
    }
    public override void OnRoomServerPlayersReady()
    {
        // Normalde burada base.OnRoomServerPlayersReady() çalışır ve sahneyi değiştirir.
        // Biz bunu SİLİYORUZ / ÇAĞIRMIYORUZ.
        
        Debug.Log("Tüm oyuncular HAZIR! Ama otomatik başlatma kapalı. Host bekleniyor...");
        
    }
    public void UpdateLobbyCodeTextUI()
    {
        var lobbyCodeText = GameObject.Find("LobbyCode").GetComponent<TextMeshProUGUI>();
        lobbyCodeText.text = "Lobi Kodu: " + currentLobbyCode;
    }

    public void CopyLobbyCode()
    {
        GUIUtility.systemCopyBuffer = currentLobbyCode;
    }
}