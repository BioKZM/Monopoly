
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using Unity.VisualScripting;

public class PlayerScript : NetworkBehaviour
{
    private List<GameObject> tiles = new();

    public MonopolyGamePlayer visualData; 

    [SyncVar(hook = nameof(OnMoneyChanged))] public int money = 50000; // Oyuncunun parası
    [SyncVar(hook = nameof(OnPlayerAllReady))] public bool isPlayerAllReady;
    
    [SyncVar] public int currentTileIndex = 0; // Oyuncunun mevcut kare indeksi
    [SyncVar] public bool hasMadeDecision = false; // Oyuncu mülk satın alımını yaptı mı?
    [SyncVar] public bool isInJail = false; // Oyuncunun hapis durumu
    public bool wantsToBuy = false; // Mülk satın alımı için UI isteği
    public readonly SyncList<string> ownedTiles = new SyncList<string>(); // Oyuncunun satın aldığı mülklerin ismi
    public bool hasRolledDice = false; // Oyuncunun zar atma durumu
    public bool isMoving = false; // Oyuncunun hareket durumu    
    public bool tookMoneyOnStart = true; // Oyuncunun başlangıçta para alma durumu
    public int jailRollCount = 0; // Oyuncunun hapisten çıkmak için zar atma sayısı, 0-3 arası artan formatta. 3 olunca zorunlu ödeme.
    public bool isBankrupt = false; // Oyuncunun iflas durumu, UI çağrısı için   
    public bool bankrupted = false; // Oyuncunun iflas durumu, kontrol için, izleyici modunda    
    public string playerName; // Oyuncunun ismi (Steam)   
    public ulong playerSteamId; // Oyuncunun Steam ID'si  
    public Texture steamAvatarTexture; // Oyuncunun Steam profil resmi
    public Color playerColor; // Oyuncunun seçtiği karakter rengi
    public int characterIndex; // Oyuncunun seçtiği karakter indeksi
    public int playerBackgroundIndex;
    public Material baseMaterial;
    public Material playerMaterial;
    public Material playerMaterialDark;
    
    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    void OnEnable() {
       // Steam callback'ini kaydet
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }
    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        // Eğer gelen avatar bu oyuncunun ID'sine aitse
        if (callback.m_steamID.m_SteamID == playerSteamId)
        {
            Debug.Log($"[STEAM] Avatar yüklendi: {playerName}");
            GetSteamAvatar(callback.m_steamID);
            
            // Resim hazır olduğu için UI'ı tekrar dürtüyoruz
            GameManager.Instance.uiManager.SetPlayersInfo(GameManager.Instance.players);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        ownedTiles.Callback += OnOwnedTilesChanged;
        FindTiles();
        StartCoroutine(PrepareAndRegister());

    }

    public void Update()
    {
        GameObject escPanel = GameManager.Instance.uiManager.gameObject;
        if (escPanel != null)
        {
            bool isActive = escPanel.activeSelf;
            escPanel.SetActive(!isActive);

            if (!isActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    private void InitializeMaterials()
    {
        if (baseMaterial == null) return;

        playerMaterial = new Material(baseMaterial)
        {
            color = playerColor,
            name = $"{playerName}_Material_{playerColor.ToHexString()}"
        };

        playerMaterialDark = new Material(baseMaterial);

        Color darkColor  = new Color(playerColor.r * 0.7f, playerColor.g * 0.7f, playerColor.b * 0.7f);

        playerMaterialDark.color = darkColor;
        playerMaterialDark.name = $"{playerName}_MaterialDark_{darkColor.ToHexString()}";

    }
    
    private bool registrationStarted = false;
    IEnumerator PrepareAndRegister()
    {
        if (registrationStarted) yield break;
        registrationStarted = true;

        var visualData = GetComponent<MonopolyGamePlayer>();
        while (visualData == null || visualData.steamID == 0)
        {
            yield return new WaitForSeconds(0.2f);
        }
    
        playerName = visualData.steamName;
        playerColor = visualData.playerColor;
        playerSteamId = visualData.steamID;
        playerBackgroundIndex = visualData.backgroundIndex;
        InitializeMaterials();
        GetSteamAvatar(new CSteamID(playerSteamId));
        if (isLocalPlayer)
        {
            CmdRegisterToManager();
            GameManager.Instance.InitializeGameLocal();
        }
    }
    

    [Command]
    public void CmdRegisterToManager()
    {
        bool alreadyExists = false;
        foreach(var player in GameManager.Instance.players)
        {
            if (player != null && player.netId == this.netId)
            {
                alreadyExists = true;
                break;
            }
        }
        if (!alreadyExists)
        {
            GameManager.Instance.players.Add(this);
            Debug.Log($"[SERVER] {playerName} (ID: {netId}) listeye başarıyla eklendi.");
        }
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        GameManager.Instance.players.Remove(this);
    }

    public void FindTiles()
    {
        tiles.Clear();
        GameObject tilesParent = GameObject.Find("Tiles");
        if (tilesParent != null)
        {
            foreach (Transform tile in tilesParent.transform)
            {
                tiles.Add(tile.gameObject);
            }
            Debug.Log($"[TILES ADDED] - {tiles.Count} adet arsa listeye eklendi.");
        }
    }

    public void UpdateOwnedTilesUI()
    {
        if (ownedTiles.Count != 0)
        {
            var userInterface = GameManager.Instance.GetUIElements().ownedCardsPanel;
            foreach (Transform child in userInterface)
            {
                child.gameObject.SetActive(false);
            }
            foreach (var tileName in ownedTiles)
            {
                var card = userInterface.Find(tileName);
                if (card != null) card.gameObject.SetActive(true);
            }
        }
    }


    // public IEnumerator MoveCoroutine(int dice)
    // {
    //     Debug.Log($"T: {dice}");
    //     Debug.Log($"[MOVEMENT] CurrentTileIndex - {currentTileIndex}");
    //     isMoving = true;

    //     if (currentTileIndex != 0 && NetworkServer.active)
    //     {
    //         tookMoneyOnStart = false; 
    //     }

    //     // int visualIndex = (currentTileIndex - dice + 40) % 40;
    //     int targetIndex = currentTileIndex; 
    //     Debug.Log($"[MOVEMENT] targetIndex - {targetIndex}");
    //     for (int x = 0; x < dice; x++)
    //     {
    //         targetIndex = (targetIndex + 1) % 40;

    //         if (isServer)
    //         {
    //             currentTileIndex = targetIndex;
    //             if (currentTileIndex == 0 && !tookMoneyOnStart)
    //             {
    //                 money += 2000;
    //                 tookMoneyOnStart = true;
    //             }
    //         }

    //         Vector3 targetPos = tiles[targetIndex].transform.position + new Vector3(0, 0.5f, 0);
    //         while (Vector3.Distance(transform.position, targetPos) > 0.01f)
    //         {
    //             transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 12f);
    //             yield return null;
    //         }
    //         if (targetIndex % 10 == 0)
    //         {
    //             yield return StartCoroutine(RotateSmoothly(targetIndex));
    //             // RotatePlayer();   
    //         }
    //     }
    //     transform.position = tiles[currentTileIndex].transform.position + new Vector3(0, 0.5f, 0);
    //     RotatePlayer();
    //     isMoving = false;
    // }
    public IEnumerator MoveCoroutine(int dice)
    {
        isMoving = true;
        int targetIndex = currentTileIndex; 

        for (int x = 0; x < dice; x++)
        {
            targetIndex = (targetIndex + 1) % 40;

            if (isServer)
            {
                currentTileIndex = targetIndex;
                // Başlangıç noktası parası kontrolü aynı kalıyor
                if (currentTileIndex == 0 && !tookMoneyOnStart)
                {
                    money += 2000;
                    tookMoneyOnStart = true;
                }
            }

            Vector3 startPos = transform.position;
            Vector3 targetPos = tiles[targetIndex].transform.position + new Vector3(0, 0.5f, 0);
            
            float elapsed = 0f;
            float jumpDuration = 0.3f; // Her zıplamanın hızı
            float jumpHeight = 1.5f;   // Ne kadar yükseğe zıplayacak?

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float percent = elapsed / jumpDuration;

                // Yatayda ilerleme (X ve Z)
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, percent);

                // Dikeyde zıplama (Y ekseni) - Sinüs dalgası
                // sin(0) = 0, sin(pi/2) = 1, sin(pi) = 0 mantığıyla çalışır
                currentPos.y += Mathf.Sin(percent * Mathf.PI) * jumpHeight;

                transform.position = currentPos;
                yield return null;
            }

            // Pozisyonu tam sabitle
            transform.position = targetPos;

            // Köşe rotasyonu kontrolü
            if (targetIndex % 10 == 0)
            {
                yield return StartCoroutine(RotateSmoothly(targetIndex));
            }
        }

        RotatePlayer();
        isMoving = false;
    }

    private IEnumerator RotateSmoothly(int cornerIndex)
    {
        // Köşeye göre hedef rotasyonu belirle
        // 0 -> 0 derece, 10 -> 90, 20 -> 180, 30 -> 270 
        float targetYAngle = (cornerIndex / 10) * 90f;
        Quaternion targetRotation = Quaternion.Euler(0, targetYAngle, 0);
        Quaternion startRotation = transform.rotation;

        float elapsed = 0f;
        float duration = 0.3f; // Dönüş hızı

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }
        transform.rotation = targetRotation;
    }

    public void RotatePlayer()
    {
        if (currentTileIndex < 10)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (currentTileIndex >= 10 && currentTileIndex < 20)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (currentTileIndex >= 20 && currentTileIndex < 30)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (currentTileIndex >= 30 && currentTileIndex < 40)
        {
            transform.rotation = Quaternion.Euler(0, 270, 0);
        }
    }
    public void MoveTo(int targetTileIndex, bool isLookingCurrentTile, bool isJail, bool goToSpawn)
    {
        if (!isServer) return;

        if (isJail)
        {
            GoToJail();
            return;
        }
        else if (goToSpawn)
        {
            currentTileIndex = 0;
            money += 2000; // Başlangıçta "Go" karesinden geçildiğinde para kazanma
            tookMoneyOnStart = false;
        }
        else if (isLookingCurrentTile)
        {
            currentTileIndex += targetTileIndex % 40;
            if (currentTileIndex < 3)
            {
                currentTileIndex += 40; // Kullanıcı geriye giderken aksilik yaşanmaması için
            }

        }
        else
        {
            currentTileIndex = targetTileIndex;
        }
        RpcTeleportPlayer(currentTileIndex);
        // transform.position = tiles[currentTileIndex].transform.position + new Vector3(0, 0.5f, 0);
    }
    public void GoToJail()
    {
        int currentPlayerIndex = GameManager.Instance.players.IndexOf(this);
        if (!isServer) return;

        // Değer ataması
        jailRollCount = 0;
        currentTileIndex = 10;
        isInJail = true;

        // Oyuncuyu bütün client'larda hapse gönder.
        RpcTeleportPlayer(10);

        GameManager.Instance.RpcShowGoToJail(currentPlayerIndex);
    }
    public void CheckBankruptcy()
    {
        if (money < 0 && !isBankrupt)
        {

            isBankrupt = true;
            if (isLocalPlayer)
            {
                GameManager.Instance.InitiateBankruptcy(this);
            }
        }

    }
    #region Network Callbacks


    [Command]
    public void CmdSetReadyStatus(bool status)
    {
        Debug.Log($"[COMMAND] CmdSetReadyStatus aktif. status : {status}");
        // 1. SyncVar'ı değiştir (Böylece herkeste OnPlayerAllReady hook'u çalışır)
        isPlayerAllReady = status;

        // 2. GameManager'daki o meşhur sayacı artır
        GameManager.Instance.CmdReportReady(); 

    }
    public void OnPlayerAllReady(bool oldValue, bool newValue)
    {
        if (!isClient) return;
        Debug.Log($"[HOOK] OnPlayerAllReady triggered. New Value : {newValue}");
        if (newValue)
        {
            if (playerSteamId != 0)
            {
                GetSteamAvatar(new CSteamID(playerSteamId));
            }
            
        }
    }


    [Command]
    public void CmdSellProperty(string tileName)
    {
        TileRuntimeData tile = GameManager.Instance.propertyManager.GetRuntimeTileByName(tileName);
        int tileIndex = GameManager.Instance.propertyManager.tileRuntimeList.FindIndex(t => t.tileData.tileName == tile.tileData.tileName);
        if (tile == null || tile.owner != this) return;

        money += GameManager.Instance.bankruptcyManager.CalculateMortgageValue(tile);
        
        if (ownedTiles.Contains(tileName))
        {
            ownedTiles.Remove(tileName);
        }


        RpcOnPropertySold(tileIndex);

    }

    [Command]
    public void CmdBankruptEverything()
    {
        
        // Oyuncunun kalan tüm mülklerini tek seferde Server'da temizleyen güvenli metod
        List<string> tilesToSell = new List<string>(ownedTiles); 
        foreach (var tName in tilesToSell)
        {
            var tile = GameManager.Instance.propertyManager.GetRuntimeTileByName(tName);
            int tileIndex = GameManager.Instance.propertyManager.tileRuntimeList.FindIndex(t => t.tileData.tileName == tile.tileData.tileName);
            if (tile != null)
            {
                this.money += GameManager.Instance.bankruptcyManager.CalculateMortgageValue(tile);
                RpcOnPropertySold(tileIndex);
            }
        }
        this.ownedTiles.Clear();
        
        // Elendiğini işaretle
        this.isBankrupt = true;
        this.bankrupted = true;
        
        RpcRemovePlayerFromGame();
    }
    [ClientRpc]
    public void RpcOnPropertySold(int tileIndex)
    {
        TileRuntimeData tile = GameManager.Instance.propertyManager.GetRuntimeTile(tileIndex);
        if (tile != null)
        {
            tile.owner = null;
            tile.hasHouse = false;
            tile.hasHotel = false;
        }    


        if (GameManager.Instance.propertyTiles.Count > tileIndex)
        {
            GameObject propertyTile = GameManager.Instance.propertyTiles[tileIndex];
            // Tüm ev ve otel görsellerini kapat
            for (int i = 0; i < propertyTile.transform.childCount; i++)
            {
                propertyTile.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }


    
    

    [ClientRpc]
    public void RpcTeleportPlayer(int newIndex)
    {
        transform.position = tiles[newIndex].transform.position + new Vector3(0, 0.5f, 0);
        RotatePlayer(); // Gittiği yerdeki yöne doğru dönmesi için
    }

    [ClientRpc]
    public void RpcRemovePlayerFromGame()
    {
        GameManager.Instance.uiManager.RemovePlayerInfoPanel(this);
    
        // Görseli gizle
        this.gameObject.SetActive(false); 
        
        // Listeden güvenli bir şekilde çıkar
        if (GameManager.Instance.players.Contains(this))
        {
            GameManager.Instance.players.Remove(this);
        }
        if (isLocalPlayer)
        {
            // Butonları tamamen kapat ki hayalet gibi oynamaya devam etmeyeyim
            GameManager.Instance.uiManager.SetGroup(null);
            Debug.Log("Elendin hacı, geçmiş olsun.");
        }
        // // var panels = GameManager.Instance.uiManager.playerInfoPanels;
        // // GameObject panelToRemove = 
        
        // gameObject.SetActive(false);
        // // var index = GameManager.Instance.players.FindIndex(p => p == this);
        // GameManager.Instance.uiManager.RemovePlayerInfoPanel(bankruptedPlayer);
    }
    
    #endregion

    #region Hooks
    
    public void OnMoneyChanged(int oldMoney, int newMoney)
    {   
        GameManager.Instance.UpdateUI();
    }

    public void OnOwnedTilesChanged(SyncList<string>.Operation op, int index, string oldItem, string newItem)
    {
        // UpdateOwnedTilesUI();
    }
    #endregion


    #region Steam Avatar Handling
    void GetSteamAvatar(CSteamID steamId)
    {
        int imageId = SteamFriends.GetLargeFriendAvatar(steamId);
        if (imageId == -1) return;
        Texture2D texture = GetSteamImageAsTexture2D(imageId);
        if (texture != null)
        {
            steamAvatarTexture = texture;
            GameManager.Instance.uiManager.SetPlayersInfo(GameManager.Instance.players);
        }
    }

    public static Texture2D GetSteamImageAsTexture2D(int iImage)
    {
        Texture2D texture = null;
        uint width, height;
        if (SteamUtils.GetImageSize(iImage, out width, out height))
        {
            byte[] image = new byte[width * height * 4];
            if (SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4)))
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        return texture != null ? FlipTexture(texture) : null;
    }

    private static Texture2D FlipTexture(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);
        int xN = original.width;
        int yN = original.height;
        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                flipped.SetPixel(i, yN - j - 1, original.GetPixel(i, j));
            }
        }
        flipped.Apply();
        return flipped;
    }
    #endregion


}