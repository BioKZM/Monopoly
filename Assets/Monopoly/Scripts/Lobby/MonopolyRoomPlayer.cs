using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Reflection;
using System.Collections.Generic;

public class MonopolyRoomPlayer : NetworkRoomPlayer
{
    // Kullanıcı adı metni için game object
    public TMP_Text nameText;
    
    // Steam profil resmi
    public RawImage profileImage;

    // Lobiden oyuna geçiş butonu
    private Button startGameButton;

    // Karakter modeli seçimi için index
    [SyncVar(hook = nameof(OnCharacterChanged))]
    public int characterIndex = 0;

    // Kullanıcı adı (Lobi & Oyun için)
    [SyncVar(hook = nameof(HandleNameChanged))]
    public string playerName;
    
    // Kullanıcı Steam ID'si
    [SyncVar(hook = nameof(HandleSteamIdChanged))]
    public ulong playerSteamId;
    
    // Karakter rengi (Lobi & Oyun içi)
    [SyncVar(hook = nameof(OnLobbyColorChanged))]
    public Color playerColor;
    // Lobi içi border
    public Image borderImage;
    // Karakter modeli seçimi için dropdown
    public TMP_Dropdown characterDropdown;

    private readonly List<string> charOptions = new() { "Car", "BMO", "Mija", "Darkin"};

    


    // Steam Avatarı
    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    public void ServerForceReady()
    {
        // NetworkRoomPlayer sınıfındaki 'readyToBegin' alanını bul
        FieldInfo field = typeof(NetworkRoomPlayer).GetField("readyToBegin", BindingFlags.Public | BindingFlags.Instance);

        if (field != null)
        {
            // Bu instance (this) üzerindeki değeri TRUE olarak ayarla
            field.SetValue(this, true);
        }
    }

    public override void OnStartClient()
    {
        // Önce base fonksiyonu çalışsın (Mirror işlerini yapsın)
        base.OnStartClient();

        // 1. Hedef Yuvayı Bul (Hiyerarşideki ismi 'LobbyContent' olan objeyi arar)
        // Not: İsmin harfiyen Unity'deki ile aynı olduğundan emin ol.
        // GameObject targetParent = GameObject.Find("LobbyContent");
        GameObject targetParent = GameObject.Find("LobbyContent");

        Debug.Log("[DEBUG - GAMEOBJECT] targetParent: " + targetParent);
        if (targetParent != null)
        {
            // 2. Babasının altına gir (Evlatlık edinilme anı)
            // 'false' parametresi çok önemli: "Dünya pozisyonunu korumaya çalışma, babana uyum sağla" demektir.
            transform.SetParent(targetParent.transform, false);
            
            // 3. Ölçek (Scale) Düzeltmesi
            // Bazen re-parent yapınca scale (100,100,100) falan olur, bunu sıfırlayalım.
            transform.localScale = Vector3.one; 
            
            Debug.Log($"[UI FIX] RoomPlayer başarıyla {targetParent.name} altına taşındı!");
        }
        else
        {
            Debug.LogWarning("[UI HATA] 'LobbyContent' objesi sahnede bulunamadı! İsim doğru mu?");
        }
        if (characterDropdown != null)
        {
            characterDropdown.ClearOptions();
            characterDropdown.AddOptions(charOptions);
            characterDropdown.RefreshShownValue();
        }
        OnCharacterChanged(0, characterIndex);
        OnLobbyColorChanged(Color.white, playerColor);
    }

    public override void OnStartLocalPlayer()
    {
        // 1. İsim ve ID Çekme (Steam veya Rastgele)
        string myName = "Player " + Random.Range(100, 999);
        ulong myId = 0;

        if(SteamManager.Initialized) 
        {
             myName = SteamFriends.GetPersonaName();
             myId = SteamUser.GetSteamID().m_SteamID;
             avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        }
        CmdSetUserData(myName, myId);

        // 2. Buton Bağlantısı
        // if (readyButton != null)
        // {
        //     readyButton.onClick.RemoveAllListeners();
        //     readyButton.onClick.AddListener(ToggleReady);
        // }

        // 3. ODA KODU GÖSTERİMİ (Sadece Host Görür)
        if (isServer && isLocalPlayer)
        {
            GameObject buttonObject = GameObject.Find("StartGameButton");
            if (buttonObject != null)
            {
                startGameButton = buttonObject.GetComponent<Button>();
                startGameButton.interactable = true;
                startGameButton.onClick.RemoveAllListeners();
                startGameButton.onClick.AddListener(() => MonopolyNetworkManager.Instance.StartGameManually());
            }
            string code = SteamLobbyController.Instance != null ? SteamLobbyController.Instance.currentLobbyCode : "HATA";
            
            GameObject codeTextObj = GameObject.Find("LobbyCodeText");
            if(codeTextObj != null) 
            {
                codeTextObj.GetComponent<TMP_Text>().text = "ODA KODU: " + code;
                GUIUtility.systemCopyBuffer = code; // Panoya kopyala
            }
            
        }
        
    }
    

    [Command]
    public void CmdSetUserData(string name, ulong id)
    {
        playerName = name;
        playerSteamId = id;
    }

    // --- HAZIR OLMA MANTIĞI (DÜZELTİLEN KISIM) ---
    
    public void ToggleReady()
    {
        // Mirror'ın kendi fonksiyonunu çağırır (readyToBegin değişkenini değiştirir)
        CmdChangeReadyState(!readyToBegin);
    }

    // OnClientReady YERİNE Update kullanıyoruz.
    // Çünkü readyToBegin değişkeni Server'dan değişince buraya anında yansır.
    // --- KARAKTER SEÇİMİ ---
    public void SelectCharacter(int index)
    {
        if (isLocalPlayer) CmdSelectCharacter(index);
    }

    [Command]
    public void CmdSelectCharacter(int index)
    {
        characterIndex = Mathf.Clamp(index, 0, 3);
    }
    
    
    
    // --- UI HOOKS ---
    void HandleNameChanged(string old, string newName) 
    { 
        if(nameText) nameText.text = newName; 
    }

    void HandleSteamIdChanged(ulong oldId, ulong newId)
    {
        if (newId != 0) GetSteamAvatar((CSteamID)newId);
    }

    // --- AVATAR YÜKLEME (Standart Kod) ---
    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == playerSteamId) GetSteamAvatar(callback.m_steamID);
    }

    void GetSteamAvatar(CSteamID steamId)
    {
        int imageId = SteamFriends.GetLargeFriendAvatar(steamId);
        if (imageId == -1) return;
        Texture2D texture = GetSteamImageAsTexture2D(imageId);
        if (texture != null && profileImage != null)
        {
            profileImage.texture = texture;
            profileImage.color = Color.white;
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

    private void OnLobbyColorChanged(Color oldColor, Color newColor)
    {
        if (borderImage != null)
        {
            borderImage.color = newColor;
        }
    }
    public void OnColorPickerUpdated(Color newColor)
    {
        if (isLocalPlayer)
        {
            CmdSetColor(newColor);
        }
    }
    [Command]
    public void CmdSetColor(Color newColor)
    {
        playerColor = newColor;
    }

    [Command]
    public void CmdSetCharacterIndex(int index)
    {
        characterIndex = index;
    }
    void OnCharacterChanged(int oldIndex, int newIndex)
    {
        if (characterDropdown != null)
        {
            characterDropdown.SetValueWithoutNotify(newIndex);
        }
        // Karakter değişince yapılacak görsel işlemler (Şapka iconu vs.)
    }
    

}