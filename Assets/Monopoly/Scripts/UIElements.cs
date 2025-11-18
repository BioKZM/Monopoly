using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct UIElements
{
    public Button rollDiceButton;
    public Button buyButton;
    public Button buyHouseButton;
    public Button buyHotelButton;
    public Button passButton;
    public Button drawerButton;
    public Button buildPassButton;
    public Transform ownedCardsPanel;
    public GameObject ownershipTextPrefab;
    public CanvasGroup rollDiceGroup;
    public CanvasGroup propertyActionGroup;
    public CanvasGroup buildGroup;
    public CanvasGroup drawerGroup;
    public CanvasGroup detailPanel;
    public List<GameObject> playerInfoPanels;
    public GameObject bankruptcyCard;
} 