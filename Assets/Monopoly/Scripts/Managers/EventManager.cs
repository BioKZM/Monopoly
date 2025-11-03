using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// EventManager: queues and displays short event banners at the top of the screen.
/// Attach to a GameObject under Canvas and assign BannerRect (background) and MessageText (TMP).
/// Use EventManager.Instance.ShowPurchase(...), ShowRentPayment(...), etc. to enqueue messages.
/// </summary>
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("UI References")]
    private RectTransform bannerRect;         // root rect of the card (anchored to top center)

    [Header("Message Text (created at runtime)")]
    // The TextMeshProUGUI is created automatically as a child of bannerRect at runtime.
    // Assign a font asset and style via the inspector below.
    private TMP_FontAsset messageFont;       // assign in inspector
    private int messageFontSize;
    private Color messageFontColor = Color.black;
    private TextMeshProUGUI messageText;      // created at Awake if null

    [Header("Animation")]
    private Vector2 offscreenAnchored;
    private Vector2 onscreenAnchored;
    private float slideDuration = 0.32f;
    private float visibleDuration = 2.2f;

    [Header("Queue")]
    private int maxQueue = 12;

    Queue<string> queue = new Queue<string>();
    bool showing = false;
    CanvasGroup canvasGroup;

    void Awake()
    {
        messageFont = GameManager.Instance.messageFont;
        messageFontSize = GameManager.Instance.messageFontSize;
        bannerRect = GameManager.Instance.bannerRect;
        maxQueue = GameManager.Instance.maxQueue;
        
        // Banner'ı ekranın dışına ve içine konumlandır
        float bannerHeight = bannerRect.rect.height;
        // Ekranın üstünde gizlenir (-yükseklik + biraz fazlası)
        offscreenAnchored = new Vector2(0, bannerHeight * 1.2f);
        // Ekranın üstünde görünür (biraz boşluk bırak)
        onscreenAnchored = new Vector2(0, 20f);

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (bannerRect == null)
        {
            Debug.LogError("EventManager: assign bannerRect in inspector.");
            enabled = false;
            return;
        }

        // ensure a CanvasGroup exists
        canvasGroup = bannerRect.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = bannerRect.gameObject.AddComponent<CanvasGroup>();

        // create messageText at runtime if not already set
        if (messageText == null)
        {
            GameObject go = new GameObject("MessageText", typeof(RectTransform));
            go.transform.SetParent(bannerRect, false);
            messageText = go.AddComponent<TextMeshProUGUI>();
            messageText.raycastTarget = false;
            // default wrapping behavior is used; adjust overflowMode in inspector if needed
            messageText.richText = true;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.fontSize = messageFontSize;
            messageText.color = messageFontColor;
            if (messageFont != null) messageText.font = messageFont;

            // stretch to banner with some padding
            var rt = messageText.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = new Vector2(8f, 8f);
            rt.offsetMax = new Vector2(-8f, -8f);
        }

        // start hidden
        bannerRect.anchoredPosition = offscreenAnchored;
        canvasGroup.alpha = 1f;
    }


    // Enqueue a pre-formatted (rich text) message
    public void Enqueue(string richText)
    {
        if (string.IsNullOrWhiteSpace(richText)) return;
        if (queue.Count >= maxQueue) queue.Dequeue();
        queue.Enqueue(richText);
        if (!showing) StartCoroutine(ShowNext());
    }

    IEnumerator ShowNext()
    {
        if (queue.Count == 0) yield break;
        showing = true;
        string msg = queue.Dequeue();
        messageText.text = msg;

        yield return Animate(offscreenAnchored, onscreenAnchored, slideDuration);
        float t = 0f;
        while (t < visibleDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        yield return Animate(onscreenAnchored, offscreenAnchored, slideDuration);
        showing = false;
        if (queue.Count > 0) StartCoroutine(ShowNext());
    }

    IEnumerator Animate(Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(0f, 1f, t / duration);
            bannerRect.anchoredPosition = Vector2.Lerp(from, to, a);
            yield return null;
        }
        bannerRect.anchoredPosition = to;
    }

    // -------------------------
    // Public helpers (templates)
    // -------------------------
    public void ShowPurchase(PlayerScript buyer, TileRuntimeData tile)
    {
        string template = "{player} {tile} arsasını satın aldı.";
        string msg = BuildMessage(template, buyer, tile, null, null);
        Enqueue(msg);
    }
    public void ShowSelling(PlayerScript seller, TileRuntimeData tile)
    {
        string template = "{player} {tile} arsasını sattı.";
        string msg = BuildMessage(template, seller, tile, null, null);
        Enqueue(msg);
    }

    public void ShowRentPayment(PlayerScript payer, PlayerScript payee, TileRuntimeData tile, int amount)
    {
        string template = "{player} {player2} oyuncusuna kira ödüyor.";
        string msg = BuildMessage(template, payer, tile, null, payee);
        msg += $" <color=#DE0000>({amount:N0}₺)</color>";
        Enqueue(msg);
    }

    public void ShowGoToJail(PlayerScript player)
    {
        string template = "{player} kodese düştü";
        Enqueue(BuildMessage(template, player, null, null, null));
    }

    public void ShowBankrupt(PlayerScript player)
    {
        string template = "{player} iflas etti";
        Enqueue(BuildMessage(template, player, null, null, null));
    }

    public void ShowBuild(PlayerScript player, TileRuntimeData tile, string buildingName)
    {
        string template = "{player} {tile} arsasına {building} dikti.";
        Enqueue(BuildMessage(template, player, tile, buildingName, null));
    }

    public void ShowTaxPayment(PlayerScript player, TileRuntimeData tile, int amount)
    {
        string template = "{player} {tile} ödüyor.";
        string message = BuildMessage(template, player, tile, null, null);
        message += $" <color=#DE0000>({amount:N0}₺)</color>";
        Enqueue(message);
    }

    public void ShowCustom(string template, PlayerScript player = null, TileRuntimeData tile = null, string building = null, PlayerScript player2 = null)
    {
        Enqueue(BuildMessage(template, player, tile, building, player2));
    }

    // -------------------------
    // Message builder + color helpers
    // -------------------------
    string BuildMessage(string template, PlayerScript player, TileRuntimeData tile, string building, PlayerScript player2)
    {
        string result = template;

        if (player != null)
        {
            var name = SafePlayerName(player);
            result = result.Replace("{player}", Colorize(name, GetPlayerColor(player)));
        }

        if (player2 != null)
        {
            var name2 = SafePlayerName(player2);
            result = result.Replace("{player2}", Colorize(name2, GetPlayerColor(player2)));
        }

        if (tile != null)
        {
            var tileName = Escape(tile.tileData.tileName);
            result = result.Replace("{tile}", Colorize(tileName, GetTileColor(tile)));
        }

        if (!string.IsNullOrEmpty(building))
        {
            result = result.Replace("{building}", Colorize(Escape(building), GetBuildingColor(building)));
        }

        return result;
    }
    Color GetBuildingColor(string building)
    {
        switch (building.ToLower())
        {
            case "ev": return new Color32(0x00, 0x80, 0x00, 255); // green
            case "otel": return new Color32(0xFF, 0x00, 0x00, 255); // red
            default: return Color.gray;
        }
    }
    string Colorize(string text, Color c)
    {
        string hex = ColorUtility.ToHtmlStringRGB(c);
        return $"<color=#{hex}>{text}</color>";
    }

    string Escape(string s)
    {
        if (s == null) return string.Empty;
        return s.Replace("<", "&lt;").Replace(">", "&gt;");
    }

    string SafePlayerName(PlayerScript p)
    {
        if (p == null) return "Player";
        if (!string.IsNullOrEmpty(p.playerName)) return Escape(p.playerName);
        if (!string.IsNullOrEmpty(p.name)) return Escape(p.name);
        return "Player";
    }

    Color GetPlayerColor(PlayerScript player)
    {
        if (player == null) return Color.white;
        // try common field names, otherwise fallback to palette by index
        var type = player.GetType();
        var f = type.GetField("playerColor");
        if (f != null)
        {
            object v = f.GetValue(player);
            if (v is Color c) return c;
        }
        // fallback palette
        int idx = GameManager.Instance.players.IndexOf(player);
        return Palette(idx);
    }

    Color GetTileColor(TileRuntimeData tile)
    {
        if (tile == null) return Color.black;
        var propertyManager = GameManager.Instance.propertyManager;
        if (propertyManager != null)
        {
            try { return propertyManager.GetTileColor(tile.tileData); } catch { }
        }
        if (tile.tileData is PropertyData propertyData)
        {
            switch (propertyData.groupColor)
            {
                case "Brown": return new Color32(0x8B,0x45,0x13,255);
                case "Light Blue": return new Color32(0x87,0xCE,0xEB,255);
                case "Pink": return new Color32(0xFF,0x69,0xB4,255);
                case "Orange": return new Color32(0xFF,0xA5,0x00,255);
                case "Red": return new Color32(0xFF,0x00,0x00,255);
                case "Yellow": return new Color32(0xFF,0xD7,0x00,255);
                case "Green": return new Color32(0x00,0x80,0x00,255);
                case "Blue": return new Color32(0x00, 0x00, 0xBB, 255);
            }
        }
        else if (tile.tileData is UoSData uoSData)
        {
            return new Color32(0xA9, 0xA9, 0xA9, 255); // DarkGray
        }
        else if (tile.tileData is TaxData)
        {
            switch (tile.tileData.tileName)
            {
                case "GelirVergisi": return new Color32(0x2E, 0x8B, 0x57, 255); // Sea Green
                case "LüksVergisi": return new Color32(0x40, 0xE0, 0xD0, 255); // Turquoise
            }
            return new Color32(0x00, 0x00, 0x00, 255); // Black
        }
        return Color.black;
    }

    Color Palette(int idx)
    {
        Color[] pal = new Color[] {
            new Color32(0x2E,0x8B,0x57,255),
            new Color32(0x1E,0x90,0xFF,255),
            new Color32(0xFF,0xA5,0x00,255),
            new Color32(0xFF,0x45,0x00,255),
            new Color32(0x8A,0x2B,0xE2,255),
            new Color32(0xDA,0xA5,0x20,255)
        };
        if (idx < 0) return Color.black;
        return pal[idx % pal.Length];
    }
}
