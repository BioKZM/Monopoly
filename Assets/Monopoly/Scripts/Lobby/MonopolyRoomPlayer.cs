using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using Mirror.Examples.Basic;

public class MonopolyRoomPlayer : NetworkRoomPlayer
{
    [Header("UI Referansları")]
    public TMP_Text nameText;
    public RawImage profileImage;
    public Image borderImage;
    public TMP_Dropdown characterDropdown;
    public int backgroundIndex = 0;

    [Header("Senkronize Veriler")]
    [SyncVar(hook = nameof(OnCharacterChanged))] public int characterIndex = 0;
    [SyncVar(hook = nameof(HandleNameChanged))] public string playerName;
    [SyncVar(hook = nameof(HandleSteamIdChanged))] public ulong playerSteamId;
    [SyncVar(hook = nameof(OnLobbyColorChanged))] public Color playerColor = Color.white;

    private readonly List<string> charOptions = new() { "Car", "BMO", "Mija", "Darkin", "RedHatRedemption", "Monkey", "Deadpool"};
    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
    
        // 1. Lobi Paneline Yerleşme
        GameObject targetParent = GameObject.Find("LobbyContent");
        if (targetParent != null)
        {
            transform.SetParent(targetParent.transform, false);
            transform.localScale = Vector3.one;
        }
        Transform localCanvas = transform.Find("LocalCanvas");

        // 2. UI Kısıtlamaları
        if (!isLocalPlayer && localCanvas != null)
        {
            localCanvas.gameObject.SetActive(false);
        }

        // 3. Dropdown Hazırlığı
        if (characterDropdown != null)
        {
            characterDropdown.ClearOptions();
            characterDropdown.AddOptions(charOptions);
            characterDropdown.RefreshShownValue();
        }


        if (SteamManager.Initialized)
        {
            // Sadece local player değil, her client bu haberi dinlemeli
            avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
            
            // Steam verisi bazen anında gelmez, isim için de bir callback eklemek hayat kurtarır
            // Bu isteğe bağlıdır ama garantiye alır:
            // Callback<PersonaStateChange_t>.Create(OnPersonaStateChange); 
        }

        if (playerSteamId != 0) 
        {
        SteamFriends.RequestUserInformation((CSteamID)playerSteamId, false);
        GetSteamAvatar((CSteamID)playerSteamId);
        }

        // 4. FORCE UPDATE: Değer değişmese bile görselleri çiz
        if (nameText != null) nameText.text = playerName;
        OnCharacterChanged(0, characterIndex);
        OnLobbyColorChanged(Color.white, playerColor);

        backgroundIndex = PlayerPrefs.GetInt("SelectedCardSkin", 0); // Kaydedilmiş arka plan indexini al
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (characterDropdown != null) characterDropdown.onValueChanged.AddListener(CmdSelectCharacter);
        
        string myName = "Player " + Random.Range(100, 999);
        ulong myId = 0;

        if (SteamManager.Initialized)
        {
            myName = SteamFriends.GetPersonaName();
            myId = SteamUser.GetSteamID().m_SteamID;
            avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
        }
        CmdSetUserData(myName, myId);

        // Host ise Başlat butonunu aktifleştir
        if (isServer)
        {
            SetupHostUI();
        }
    }

    private void SetupHostUI()
    {
        GameObject buttonObject = GameObject.Find("StartGameButton");
        if (buttonObject != null)
        {
            var button = buttonObject.GetComponent<Button>();
            if (!isServer)
            {
                buttonObject.SetActive(false);
            }
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => MonopolyNetworkManager.Instance.StartGameManually());
        }
        GameObject copyButtonObject = GameObject.Find("CopyButton");
        if (copyButtonObject != null)
        {
            var button = copyButtonObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => MonopolyNetworkManager.Instance.CopyLobbyCode());
        
        }
    }

    
    #region Commands
    // --- KOMUTLAR (SERVER'DA ÇALIŞIR) ---
    [Command]
    public void CmdSetUserData(string name, ulong id) 
    { 
        playerName = name; playerSteamId = id; 
    }

    [Command]
    public void CmdSelectCharacter(int index) 
    { 
        Debug.Log("OnCharacterChangedTriggered, new Value: " + index);
        characterIndex = Mathf.Clamp(index, 0, charOptions.Count - 1);
    }

    [Command]
    public void CmdSetColor(Color newColor) 
    { 
        playerColor = newColor; 
    }
    #endregion

    #region Hooks
    // --- HOOKS (GÖRSEL GÜNCELLEMELER) ---
    void OnCharacterChanged(int oldIndex, int newIndex)
    {
        if (characterDropdown != null) characterDropdown.SetValueWithoutNotify(newIndex);
        Debug.Log("[HOOK VALUE CHANGE] OnCharacterChanged triggered. New value: " + newIndex);
        // characterDropdown.onValueChanged.AddListener
    }

    void OnLobbyColorChanged(Color oldColor, Color newColor)
    {
        if (borderImage != null) borderImage.color = newColor;
    }

    void HandleNameChanged(string old, string newName) 
    { 
        if (nameText) nameText.text = newName; 
    }

    void HandleSteamIdChanged(ulong oldId, ulong newId) 
    { 
        if (newId != 0) 
        {
            if (SteamManager.Initialized)
            {
                SteamFriends.RequestUserInformation((CSteamID)newId, false);
            }
            // Eğer UI henüz hazır değilse, bir frame bekleyip öyle çek
            StartCoroutine(FetchAvatarWhenReady(newId));
        }
    }

    private IEnumerator FetchAvatarWhenReady(ulong id)
    {
        // ProfileImage referansı gelene kadar bekle
        yield return new WaitUntil(() => profileImage != null);
        GetSteamAvatar((CSteamID)id);
}

    public void ServerForceReady()
    {
        FieldInfo field = typeof(NetworkRoomPlayer).GetField("readyToBegin", BindingFlags.Public | BindingFlags.Instance);
        if (field != null) field.SetValue(this, true);
    }
    #endregion



    #region Steam Avatar Handler
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
    #endregion

}