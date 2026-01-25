using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement; // Bunu unutma!
using Mirror.FizzySteam;

public class MonopolyNetworkManager : NetworkRoomManager
{
    public static MonopolyNetworkManager Instance => singleton as MonopolyNetworkManager;

    // --- BU KISIM EKSİKTİ, GERİ EKLİYORUZ ---
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
            Debug.Log("--- [MANUEL SPAWN] Oyun Sahnesi Doğrulandı. Piyon Kontrolü Yapılıyor... ---");

            // Her oyuncu için kontrol et
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
            {
                // Eğer oyuncunun zaten bir piyonu (identity) varsa ve bu piyon BMO değilse (yani hala Lobi kartıysa)
                // Veya hiç piyonu yoksa...
                if (conn.identity == null || conn.identity.GetComponent<MonopolyRoomPlayer>() != null)
                {
                    Debug.Log($"[SPAWN] {conn.connectionId} ID'li oyuncu için BMO yaratılıyor...");
                    
                    // A. Transform Ayarla (Varsa StartPosition, Yoksa 0,0,0)
                    Transform startPos = GetStartPosition();
                    Vector3 pos = startPos != null ? startPos.position : Vector3.zero;
                    Quaternion rot = startPos != null ? startPos.rotation : Quaternion.identity;

                    // B. Yarat (Instantiate)
                    // DİKKAT: playerPrefab, Inspector'daki "Player Prefab" (BMO) kutusudur.
                    GameObject gamePlayer = Instantiate(playerPrefab, pos, rot);

                    // C. Veri Transferi (Ruh Göçü)
                    // Lobi kartındaki verileri alıp yeni piyona aktarır
                    // (conn.identity şu anki lobi kartıdır)
                    if (conn.identity != null)
                    {
                        // Senin override ettiğin fonksiyonu elle çağırıyoruz
                        OnRoomServerSceneLoadedForPlayer(conn, conn.identity.gameObject, gamePlayer);
                        
                        // Eski Lobi kartını yok et (Room Manager bunu normalde otomatik yapar)
                        NetworkServer.Destroy(conn.identity.gameObject);
                    }

                    // D. Oyuncuyu Yeni Bedene Geçir
                    NetworkServer.ReplacePlayerForConnection(conn, gamePlayer);
                }
            }
        }
    }
    // --- OTO-START İPTALİ ---
    
    // ----------------------------------------

    // Veri aktarımı (Burası aynı kalıyor)
    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        var lobbyScript = roomPlayer.GetComponent<MonopolyRoomPlayer>();
        var gameScript = gamePlayer.GetComponent<PlayerScript>(); 

        if (lobbyScript != null && gameScript != null)
        {
            // Rengi Aktar
            gameScript.playerColor = lobbyScript.playerColor;
            
            // Karakter Indexini Aktar
            gameScript.characterIndex = lobbyScript.characterIndex;
            
            // İsim vs. aktar...
        }

        return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
    }
    // --- MANUEL BAŞLATMA (Host Butonuna Bağlanacak) ---
    public void StartGameManually()
    {
        if (!NetworkServer.active) return; // Sadece Host yapabilir

        // 1. GÜVENLİK: Odadaki herkesi ZORLA 'Hazır' yap
        // Böylece Mirror "Bu adam hazır değildi" diyip piyonu unutmamazlık yapamaz.
        foreach (var player in roomSlots)
        {
            if (player != null)
            {
                var monopolyPlayer = player as MonopolyRoomPlayer;
                if (monopolyPlayer != null)
                {
                    monopolyPlayer.ServerForceReady();
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
        
        // Buraya UI tetikleyicisi koyacağız (Adım 3'te)
    }
}