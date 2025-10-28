
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    public List<GameObject> tiles;

    private Animator animator;
    public int currentTileIndex = 0;
    public bool hasMadeDecision = false;
    public bool wantsToBuy = false;
    public List<string> ownedTiles = new List<string>();
    public int money = 50000;
    public bool isMoving = false;
    public bool isInJail = false;
    public int jailRollCount = 0;
    public bool didPlayerTakeMoneyOnStart = true;
    public string playerName;


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
    }
    public void CheckBankruptcy()
    {
        if (money < 0)
        {
            Debug.Log(this);
            GameManager.Instance.InitiateBankruptcy(this);
        }

    }
}