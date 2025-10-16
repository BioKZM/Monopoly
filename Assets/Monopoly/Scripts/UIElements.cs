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
    public Transform ownedCardsPanel;
    public CanvasGroup rollDiceGroup;
    public CanvasGroup propertyActionGroup;
    public CanvasGroup buildGroup;
    public List<GameObject> playerInfoPanels;
} 