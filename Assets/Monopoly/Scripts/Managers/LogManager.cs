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

        // 2. En aşağıya ekle
        newLog.transform.SetAsLastSibling();

        // 3. Otomatik kaydırma (Hemen aşağıya odaklan)
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }


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