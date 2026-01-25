
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerScript : NetworkBehaviour
{
    public List<GameObject> tiles;

    private Animator animator;

    // Oyuncunun mevcut kare indeksi
    public int currentTileIndex = 0;
    // Oyuncu mülk satın alımını yaptı mı?
    public bool hasMadeDecision = false;
    // Mülk satın alımı için UI isteği
    public bool wantsToBuy = false;
    // Oyuncunun satın aldığı mülklerin ismi
    public List<string> ownedTiles = new List<string>();
    // Oyuncunun parası
    public int money = 50000;
    // Oyuncunun hareket durumu
    public bool isMoving = false;
    // Oyuncunun hapis durumu
    public bool isInJail = false;
    // Oyuncunun hapisten çıkmak için zar atma sayısı, 0-3 arası artan formatta. 3 olunca zorunlu ödeme.
    public int jailRollCount = 0;
    // Oyuncunun başlangıçta para alma durumu
    public bool didPlayerTakeMoneyOnStart = true;
    // Oyuncunun iflas durumu, UI çağrısı için
    public bool isBankrupt = false;
    // Oyuncunun ismi (Steam)
    public string playerName;
    // Oyuncunun Steam ID'si
    public ulong playerSteamId;
    // Oyuncunun Steam profil resmi
    public RawImage playerSteamProfileImage;
    // Oyuncunun seçtiği karakter rengi
    public Color playerColor;
    // Oyuncunun seçtiği karakter indeksi
    public int characterIndex;



    void Start()
    {

        animator = transform.GetComponent<Animator>();
        playerName = gameObject.name;

    }

    void FixedUpdate()
    {

    }
    public void UpdateOwnedTilesUI()
    {
        if (ownedTiles.Count != 0)
        {
            var userInterface = GameManager.Instance.GetUIElements().ownedCardsPanel;
            foreach (var tileName in ownedTiles)
            {
                userInterface.Find(tileName).gameObject.SetActive(true);
            }
        }
    }

    public IEnumerator MoveCoroutine(int dice)
    {
        if (currentTileIndex == 0)
        {
            if (!didPlayerTakeMoneyOnStart)
            {
                money += 2000; // Assuming 2000 is the amount received for passing or landing on "Go"
                didPlayerTakeMoneyOnStart = true;
            }
        }
        for (int x = 0; x < dice; x++)
        {
            if (currentTileIndex > 0 && didPlayerTakeMoneyOnStart)
            {
                didPlayerTakeMoneyOnStart = false;
            }
            isMoving = true;
            currentTileIndex++;
            if (currentTileIndex % 10 == 0)
            {
                transform.Rotate(0, 90, 0);
            }
            currentTileIndex = currentTileIndex % 40;
            transform.position = tiles[currentTileIndex].transform.position + new Vector3(0, 0.5f, 0);
            yield return new WaitForSeconds(0.5f);
        }
        isMoving = false;
    }
    public void RotatePlayer()
    {
        if (currentTileIndex < 10)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (currentTileIndex >= 10 && currentTileIndex < 20)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else if (currentTileIndex >= 20 && currentTileIndex < 30)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (currentTileIndex >= 30 && currentTileIndex < 40)
        {
            transform.rotation = Quaternion.Euler(0, 270, 0);
        }
    }
    public void MoveTo(int targetTileIndex, bool isLookingCurrentTile, bool isJail, bool goToSpawn)
    {
        if (isJail)
        {
            GoToJail();
        }
        else if (goToSpawn)
        {
            currentTileIndex = 0;
            money += 2000; // Başlangıçta "Go" karesinden geçildiğinde para kazanma
            didPlayerTakeMoneyOnStart = false;
        }
        else if (isLookingCurrentTile)
        {
            currentTileIndex += targetTileIndex % 40;
            if (currentTileIndex < 3)
            {
                currentTileIndex += 40; // Kullanıcı geriye giderken aksilik yaşanmaması için
            }

        }
        else
        {
            currentTileIndex = targetTileIndex;
        }
        transform.position = tiles[currentTileIndex].transform.position + new Vector3(0, 0.5f, 0);
    }
    public void GoToJail()
    {
        jailRollCount = 0;
        currentTileIndex = 10;
        isInJail = true;
        transform.position = tiles[currentTileIndex].transform.position + new Vector3(0, 0.5f, 0);
        RotatePlayer();
        GameManager.Instance.ShowGoToJail(this);
    }
    public void CheckBankruptcy()
    {
        if (money < 0)
        {
            GameManager.Instance.InitiateBankruptcy(this);
            isBankrupt = true;
        }

    }
}