using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class MonopolyGamePlayer : NetworkBehaviour
{
    [Header("Görsel Referanslar")]
    public Transform modelHolder; // Modellerin spawn olacağı boş nokta (Container'ın içi)
    public List<GameObject> characterPrefabs; // Inspector'dan atacağın 4 Karakter (Car, BMO, Mija, Darkin)

    [Header("Lobi Verileri")]
    // Hook: Veri değiştiği an (veya spawn olunca) bu fonksiyon çalışsın
    [SyncVar(hook = nameof(OnCharacterChanged))] public int characterIndex;
    [SyncVar(hook = nameof(OnColorChanged))] public Color playerColor;
    [SyncVar] public ulong steamID;
    [SyncVar] public string steamName;
    [SyncVar] public int backgroundIndex;
    
    #region Hooks
    void OnCharacterChanged(int oldIndex, int newIndex) => SetupModel(newIndex);
    void OnColorChanged(Color oldColor, Color newColor) => ApplyColor(newColor);
    #endregion


    public override void OnStartClient()
    {
        base.OnStartClient();
        RefreshVisuals();
        SetBackground();
    }


    public void RefreshVisuals()
    {
        SetupModel(characterIndex);
        ApplyColor(playerColor);
    
    }

    private void SetupModel(int index)
    {
        foreach (Transform child in modelHolder) Destroy(child.gameObject);
        if (index >= 0 && index < characterPrefabs.Count)
        {
            GameObject model = Instantiate(characterPrefabs[index], modelHolder);
            model.transform.localPosition = Vector3.zero;
        }
    }
    public void SetBackground()
    {
        backgroundIndex = PlayerPrefs.GetInt("SelectedCardSkin");

    }

    public void ApplyColor(Color color)
    {
        
    }



}