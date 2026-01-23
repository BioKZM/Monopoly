using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField joinCodeInput; // Oyuncunun kodu yazdığı yer
    public Button hostButton;
    public Button joinButton;

    void Start()
    {
        // Host Butonu -> Lobi Kur
        hostButton.onClick.AddListener(() => 
        {
            SteamLobbyController.Instance.HostLobby();
        });

        // Join Butonu -> Kodu Ara
        joinButton.onClick.AddListener(() => 
        {
            string code = joinCodeInput.text.ToUpper().Trim(); // Büyük harfe çevir
            SteamLobbyController.Instance.JoinLobbyByCode(code);
        });
    }
}