using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BankruptcyManager : MonoBehaviour
{
    public static BankruptcyManager Instance { get; private set; }
    public PlayerScript bankruptedPlayer { get; private set; }
    private CanvasGroup detailPanel;
    private Transform bankruptcyPanel;
    
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
    }
    private void Start()
    {
        
    }
    public void InitalizeUI()
    {
        detailPanel = GameManager.Instance.uiManager.detailPanel;
        bankruptcyPanel = detailPanel.transform.Find("BankruptcyPanel");
        bankruptcyPanel.Find("ConfirmButton").GetComponent<Button>().onClick.AddListener(() => CheckBankruptcyResolution());
    }

    public void InitiateBankruptcy(PlayerScript player)
    {
        bankruptedPlayer = player;
        int bankruptedPlayerIndex = GameManager.Instance.players.IndexOf(bankruptedPlayer);
        detailPanel.gameObject.SetActive(true);
        bankruptcyPanel.gameObject.SetActive(true);
        
        var contentPanel = bankruptcyPanel.transform.Find("Scroll View/Viewport/Content");
        
        // Mevcut kartları temizle
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        var bankruptcyCardPrefab = GameManager.Instance.uiManager.bankruptcyCard;

        // Satın alınmış arsaları kart formunda content panel içine ekle
        foreach (var tileName in bankruptedPlayer.ownedTiles)
        {
            var tile = GameManager.Instance.propertyManager.tileRuntimeList.Find(t => t.tileData.tileName == tileName);
            if (tile != null)
            {
                Color tileColor = GameManager.Instance.GetTileColor(tile.tileData);
                Color textColor = GameManager.Instance.GetTextColor(tile);
                int mortgageValue = CalculateMortgageValue(tile);
                int currentValue = CalculateCurrentValue(tile);
                GameObject instance = Instantiate(bankruptcyCardPrefab, contentPanel, false);

                instance.transform.Find("ColorPanel").GetComponent<Image>().color = tileColor;
                Transform tilePanel = instance.transform.Find("ColorPanel/TileName");
                var tileText = tilePanel.GetComponent<TextMeshProUGUI>();
                tileText.text = tile.tileData.tileName;
                tileText.color = textColor;

                instance.transform.Find("OGValue").GetComponent<TextMeshProUGUI>().text = currentValue.ToString() + "₺";
                instance.transform.Find("MGValue").GetComponent<TextMeshProUGUI>().text = mortgageValue.ToString() + "₺";
                var sellButton = instance.transform.Find("SellButton").GetComponent<Button>();

                sellButton.onClick.AddListener(() =>
                {
                    bankruptedPlayer.CmdSellProperty(tile.tileData.tileName);
                    UpdateBankruptcyUI();

                    Destroy(instance);

                    GameManager.Instance.RpcShowSelling(bankruptedPlayerIndex, tile.tileData.tileName);
                });
            }
        }
        
    }

   
    private void CheckBankruptcyResolution()
    {
        int bankruptedPlayerIndex = GameManager.Instance.players.IndexOf(bankruptedPlayer);
        if (bankruptedPlayer.money < 0)
        {
            bankruptedPlayer.CmdBankruptEverything();
            
            // Oyuncuyu oyundan çıkar
            GameManager.Instance.RpcShowBankrupt(bankruptedPlayerIndex);
            // GameManager.Instance.RemovePlayerFromGame(bankruptedPlayer);
            bankruptedPlayer.RpcRemovePlayerFromGame();
        }
        bankruptedPlayer.isBankrupt = false;
        GameManager.Instance.uiManager.CloseDetailPanel();
        
    }
    
    public void UpdateBankruptcyUI()
    {
        string currentMoney = GameManager.Instance.uiManager.FormatMoney(bankruptedPlayer.money);
        
        var currentMoneyText = "Mevcut bakiyen:\n" + currentMoney + "₺";
        bankruptcyPanel.Find("CurrentMoney").GetComponent<TextMeshProUGUI>().text = currentMoneyText;
    }

    

    public int CalculateMortgageValue(TileRuntimeData tile)
    {
        return CalculateCurrentValue(tile) * 85 / 100; // %85 mortgage değeri
    }

    // Arsanın mevcut değerini hesapla (evler ve oteller dahil)
    private int CalculateCurrentValue(TileRuntimeData tile)
    {
        int currentValue = 0;

        if (tile.tileData is PropertyData property)
        {
            currentValue += property.price + (tile.hasHouse ? property.houseCost : 0) + (tile.hasHotel ? property.hotelCost : 0);
        }
        else if (tile.tileData is UoSData uOs)
        {
            currentValue = uOs.price;
        }
        return currentValue;

    }
    private void DeleteCardFromBankruptcyUI(Button sellButton)
    {
        Destroy(sellButton.transform.parent.gameObject);
    }
}
