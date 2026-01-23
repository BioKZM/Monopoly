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


    private MonopolyRoomPlayer localPlayerScript;

    void Start()
    {
        // Sliderlar değişince çalışacak fonksiyonu bağla
        sliderR.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        sliderG.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        sliderB.onValueChanged.AddListener(delegate { OnSliderChanged(); });
        hexInputField.onEndEdit.AddListener(delegate { OnHexInputSubmit(hexInputField.text); });
    }

    // Slider her oynadığında burası çalışır
    public void OnSliderChanged()
    {
        // 1. Yeni rengi oluştur
        Color newColor = new Color(sliderR.value, sliderG.value, sliderB.value, 1f);

        // 2. Önizleme kutusunu boya
        if (previewImage != null) previewImage.color = newColor;

        // 3. Oyuncuya (Server'a) gönder
        SendColorToPlayer(newColor);
    }

    void SendColorToPlayer(Color color)
    {
        // Eğer referansı daha önce almadıysak bulalım
        if (localPlayerScript == null)
        {
            var localPlayer = NetworkClient.localPlayer;
            if (localPlayer != null)
            {
                localPlayerScript = localPlayer.GetComponent<MonopolyRoomPlayer>();
            }
        }

        // Scripti bulduysak rengi gönderelim
        if (localPlayerScript != null)
        {
            // Senin RoomPlayer içinde yazdığımız fonksiyon:
            localPlayerScript.OnColorPickerUpdated(color); 
        }
    }
    public void OnHexInputSubmit(string hexString)
    {
        if (!hexString.StartsWith("#"))
        {
            hexString = "#" + hexString;
        }
        Color newColor;
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