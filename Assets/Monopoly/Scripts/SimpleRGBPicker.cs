using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class SimpleRGBPicker : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;
    public TMP_InputField hexInputField;
    public Image previewImage; // Oluşan rengi gösteren kutu

    private MonopolyRoomPlayer localPlayerScript; // MonopolyRoomPlayer referansı

    void Start()
    {
        // Sliderlar değişince çalışacak fonksiyonların bağlanması
        sliderR.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        sliderG.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        sliderB.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        hexInputField.onEndEdit.AddListener(delegate { OnHexInputSubmit(hexInputField.text); });
        
    }

    // Slider her oynadığında rengi değiştir
    public void OnSliderChanged()
    {
        Color newColor = new Color(sliderR.value, sliderG.value, sliderB.value, 1f);
        if (previewImage != null) previewImage.color = newColor;
        SendColorToPlayer(newColor);
    }

    void SendColorToPlayer(Color color)
    {

        if (localPlayerScript == null)
        {
            var localPlayer = NetworkClient.localPlayer;
            if (localPlayer != null)
            {
                localPlayerScript = localPlayer.GetComponent<MonopolyRoomPlayer>();
            }
        }

        if (localPlayerScript != null)
        {
            localPlayerScript.CmdSetColor(color);
        }
    }

    // HEX input alanına değer girildiğinde çalışır
    public void OnHexInputSubmit(string hexString)
    {
        // HEX kodunun başında # yoksa ekle
        if (!hexString.StartsWith("#"))
        {
            hexString = "#" + hexString;
        }
        Color newColor;

        // HEX kodunu Color'a çevir
        if (ColorUtility.TryParseHtmlString(hexString, out newColor))
        {
            if (sliderR) sliderR.value = newColor.r;
            if (sliderG) sliderG.value = newColor.g;
            if (sliderB) sliderB.value = newColor.b;
            if (previewImage) previewImage.color = newColor;
            SendColorToPlayer(newColor);

        }
        else
        {
            Debug.LogWarning("Geçersiz HEX renk kodu: " + hexString);
        }
    }
}