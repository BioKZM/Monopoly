using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;
using Unity.VisualScripting;
using System.Collections;

public class SteamLobbyController : MonoBehaviour
{
    public static SteamLobbyController Instance;

    // Steam Callbacks (Olay Dinleyicileri)
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    protected Callback<LobbyMatchList_t> lobbyListReturned;

    // Değişkenler
    public string currentLobbyCode;
    private const string HostAddressKey = "HostAddress";
    private const string LobbyCodeKey = "JoinCode"; // Steam'de arayacağımız etiket
    
    // public string lobbyCode;
    public TextMeshProUGUI lobbyCodeText;
    private MonopolyNetworkManager manager;


    void Start()
    {
        if (Instance == null) Instance = this;
        manager = MonopolyNetworkManager.Instance;

        if (!SteamManager.Initialized) return;

        // Callback'leri Bağla
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        lobbyListReturned = Callback<LobbyMatchList_t>.Create(OnLobbyListReturned);
    }

    // --- 1. HOST: LOBİ KURMA ---
    public void HostLobby()
    {
        // Public yapıyoruz ki kod ile aranabilsin (SpaceWar'da FriendsOnly bazen aramada çıkmaz)
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, manager.maxConnections);
        
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Lobi kurulamadı!");
            return;
        }

        Debug.Log("Steam Lobisi Kuruldu! ID: " + callback.m_ulSteamIDLobby);

        // A. Mirror Host'u Başlat
        manager.StartHost();

        // B. Host Adresini Kaydet (FizzySteamworks buna bağlanacak)
        string hostAddress = SteamUser.GetSteamID().ToString();
        // hostAddress = "localhost";
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, hostAddress);


        // C. 8 Haneli Rastgele Kod Üret ve Kaydet
        currentLobbyCode = GenerateRandomCode(8);
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), LobbyCodeKey, currentLobbyCode);


        Debug.Log($"---> ODA KODU OLUŞTURULDU: {currentLobbyCode} <---");
    }

    // --- 2. JOIN: KOD İLE ARAMA ---
    public void JoinLobbyByCode(string codeInput)
    {
        if (string.IsNullOrEmpty(codeInput)) return;
        
        Debug.Log($"Kod aranıyor: {codeInput}");

        // Filtre: "JoinCode" anahtarı benim girdiğim koda eşit olanları getir
        SteamMatchmaking.AddRequestLobbyListStringFilter(LobbyCodeKey, codeInput, ELobbyComparison.k_ELobbyComparisonEqual);
        
        // Sadece 1 sonuç yeterli
        SteamMatchmaking.AddRequestLobbyListResultCountFilter(1);
        
        SteamMatchmaking.RequestLobbyList();
    }

    private void OnLobbyListReturned(LobbyMatchList_t callback)
    {
        if (callback.m_nLobbiesMatching == 0)
        {
            Debug.LogWarning("HATA: Bu kodla eşleşen oda bulunamadı!");
            // Buraya bir UI uyarısı eklersin: "Kod Hatalı!"
            return;
        }

        // Bulunan ilk lobiye gir
        CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(0);
        SteamMatchmaking.JoinLobby(lobbyID);
    }

    // --- 3. DAVET İLE KATILMA ---
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    // --- 4. LOBİYE GİRİŞ VE BAĞLANTI ---
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Eğer Host isek zaten içerideyiz, tekrar bağlanma
        if (NetworkServer.active) return;

        // Lobi verisinden Host'un adresini (SteamID) çek
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        
        // Kodu da çek (UI'da göstermek için)
        currentLobbyCode = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), LobbyCodeKey);
        
        Debug.Log($"Lobiye Girildi. Host: {hostAddress}. Mirror Bağlanıyor...");
        
        // // Mirror'a hedefi göster ve Client'ı başlat
        StartCoroutine(ConnectAfterCheck(callback));
        // manager.networkAddress = hostAddress;
        // manager.StartClient();
    }

    private IEnumerator ConnectAfterCheck(LobbyEnter_t callback) 
    {
        Debug.Log("[LOBBY] Bağlantı kontrolü başladı...");
        
        // 1. Singleton beklerken sonsuz döngüye girmeyelim
        float timeout = 5f;
        while (NetworkManager.singleton == null && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (NetworkManager.singleton == null)
        {
            Debug.LogError("[KRİTİK] NetworkManager bulunamadı! Singleton NULL.");
            yield break;
        }

        // 2. Steam verisini kontrol et
        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(lobbyID, "HostAddress");

        if (string.IsNullOrEmpty(hostAddress))
        {
            Debug.LogError("[STEAM] HostAddress lobiden çekilemedi! Lobi verisi boş.");
            yield break;
        }

        Debug.Log($"[BAĞLANIYOR] Adres: {hostAddress}");
        NetworkManager.singleton.networkAddress = hostAddress;
        NetworkManager.singleton.StartClient();
    }
        

    // --- YARDIMCI: KOD ÜRETİCİ ---
    private string GenerateRandomCode(int length)
    {
        // Karışıklık olmasın diye O, 0, I, 1 gibi karakterleri çıkardım
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        char[] stringChars = new char[length];
        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[Random.Range(0, chars.Length)];
        }
        return new string(stringChars);
    }

}