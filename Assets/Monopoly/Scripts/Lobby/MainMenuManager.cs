using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField joinCodeInput; // Oyuncunun kodu yazdığı yer
    public Button host_Button;
    public Button join_Button;
    public Button customize_Button;
    public Button exitGame_Button;
    public Button closeLobbyPanel_Button;
    public Button closeCustomizePanel_Button;
    public Button joinLobby_Button;
    public TMP_InputField lobbyCode_Input;
    public GameObject joinLobby_Panel;
    public GameObject customize_Panel;
    void Start()
    {
        // Host Butonu -> Lobi Kur
        host_Button.onClick.AddListener(() => 
        {
            SteamLobbyController.Instance.HostLobby();
        });


        // Join Butonu -> Kodu Ara
        join_Button.onClick.AddListener(() => 
        {
            joinLobby_Panel.SetActive(true);            
            // string code = joinCodeInput.text.ToUpper().Trim(); // Büyük harfe çevir
            // SteamLobbyController.Instance.JoinLobbyByCode(code);
        });

        joinLobby_Button.onClick.AddListener(() =>
        {
            string code = lobbyCode_Input.text.ToUpper().Trim(); // Büyük harfe çevir
            SteamLobbyController.Instance.JoinLobbyByCode(code);
        });
        
        customize_Button.onClick.AddListener(() =>
        {
            customize_Panel.SetActive(true);
            ScrollRect myScrollRect = GetComponentInChildren<ScrollRect>();
            if (myScrollRect != null)
            {
                // 1.0f en üst, 0.0f en alttır
                myScrollRect.verticalNormalizedPosition = 1f; 
            }
        });

        closeLobbyPanel_Button.onClick.AddListener(() =>
        {
            closeLobbyPanel_Button.transform.parent.gameObject.SetActive(false);
        });

        closeCustomizePanel_Button.onClick.AddListener(() =>
        {
            closeCustomizePanel_Button.transform.parent.gameObject.SetActive(false);
        });

        exitGame_Button.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    

    }
}