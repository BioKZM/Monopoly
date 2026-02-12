using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance;
    
    // public TMP_Text logTemplate; // Sahnede duran pasif bir Text objesi
    public Transform contentParent;
    public ScrollRect scrollRect;
    public GameObject logPanel;
    public Button logPanelButton;
    public TextMeshProUGUI logEntryPrefab;


    private void Awake() 
    { 
        Instance = this; 
        // var logPanel = GameManager.Instance.uiManager.logsWindow;
        
    }

    public void SetupLogManager(Transform contentParent, ScrollRect scrollRect, GameObject logPanel, Button logPanelButton,TextMeshProUGUI logEntryPrefab)
    {
        this.contentParent = contentParent;
        this.scrollRect = scrollRect;
        this.logPanel = logPanel;
        this.logPanelButton = logPanelButton;
        this.logEntryPrefab = logEntryPrefab;

        logPanelButton.onClick.AddListener(() => ToggleLogbook());
    }
    public void AddLog(string message)
    {
        // 1. Template'den yeni bir tane üret
        TMP_Text newLog = Instantiate(logEntryPrefab, contentParent);
        newLog.text = message;
        newLog.gameObject.SetActive(true);
        
        // 2. İçeriği doldur
        // newLog.text = $"<color=#888888>[{System.DateTime.Now:HH:mm}]</color> {message}";
        // newLog.text = message;

        // 3. En aşağıya ekle
        newLog.transform.SetAsLastSibling();

        // 4. Otomatik kaydırma (Hemen aşağıya odaklan)
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // public GameObject CreateNewTextObject(string message)
    // {
    //     GameObject go = new GameObject("LogEntry");
    //     go.transform.SetParent(contentParent);
        
    //     TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
    //     txt.text = message;
    //     txt.fontSize = 24;
    //     txt.raycastTarget = false;
    //     return go;
        
    // }

    public void ToggleLogbook()
    {

        logPanel.SetActive(!logPanel.activeSelf);
        if(logPanel.activeSelf)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}