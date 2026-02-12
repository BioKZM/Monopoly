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




    // --- 1. MODELİ OLUŞTURMA ---
    // void OnCharacterChanged(int oldIndex, int newIndex)
    // {
        // Debug.Log("[HOOK VALUE CHANGE - GamePlayer] OnCharacterChanged triggered. New value: " + newIndex);
        // // Önce eski model varsa temizle
        // foreach (Transform child in modelHolder)
        // {
        //     Destroy(child.gameObject);
        // }

        // // Yeni index geçerli mi?
        // if (newIndex >= 0 && newIndex < characterPrefabs.Count)
        // {
        //     // Seçilen karakteri "ModelHolder"ın altına yarat
        //     GameObject model = Instantiate(characterPrefabs[newIndex], modelHolder);
            
        //     // Pozisyonu sıfırla ki Container'ın tam ortasında dursun
        //     model.transform.localPosition = Vector3.zero;
        //     model.transform.localRotation = Quaternion.identity;
            
        //     // Modeli yarattıktan sonra rengini de güncelle (Gecikme olmasın)
        //     // UpdateModelColor(model, playerColor);
        // }
    // }

    // --- 2. RENGİ GÜNCELLEME ---
    // void OnColorChanged(Color oldColor, Color newColor)
    // {
    //     Debug.Log("[HOOK VALUE CHANGE - GamePlayer] OnColorChanged triggered. New value: " + newColor);
    //     // Şu anki aktif modeli bul
    //     if (modelHolder.childCount > 0)
    //     {
    //         GameObject currentModel = modelHolder.GetChild(0).gameObject;
    //         // UpdateModelColor(currentModel, newColor);
    //     }
    // }

    // void UpdateModelColor(GameObject model, Color col)
    // {
    //     // Modelin üzerindeki tüm Renderer'ları bul (Mesh Renderer, Skinned Mesh Renderer)
    //     Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        
    //     foreach (var rend in renderers)
    //     {
    //         // Materyalin rengini değiştir
    //         rend.material.color = col;
    //     }
    // }
}