using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
public class CardManager : MonoBehaviour
{
    #region Card Data
    public List<CardData> chanceCardEffects;
    public List<CardData> communityCardEffects;
    private Dictionary<int, string> chanceData;
    private Dictionary<int, string> communityData;
    #endregion

    #region Card Management


    void Start()
    {
        chanceData = LoadCardData(isChanceData: true);
        communityData = LoadCardData(isChanceData: false);
    }

    private Dictionary<int, string> LoadCardData(bool isChanceData)
    {
        var chanceDataDict = new Dictionary<int, string>
        {

            {0, "ÜÇ HANE GERİ GİT"},
            {1 , "KODESE GİR"},
            {2 , "HIZ CEZASI\n<color=#BB0000><size=60>500TL </size></color> ÖDE"},
            {3 , "YENİKÖY'E İLERLE"},
            {4 , "YÖNETİM KURULU BAŞKANI SEÇİLDİN\nHER OYUNCUYA <color=#BB0000><size=60>500TL</size></color> ÖDE"},
            {5 , "BEYOĞLU'NA İLERLE"},
            {6 , "BANKADAN <color=#00BB00><size=60>1000TL</size></color> KAR PAYI AL"},
            {7 , "HAYDARPAŞA TREN İSTASYONU'NA İLERLE"},
            {8 , "CADDEBOSTAN'A İLERLE"},
            {9 , "TÜM BİNALARINA GENEL ONARIM YAP\n HER EV İÇİN <color=#BB0000><size=60>500TL</size></color> HER OTEL İÇİN <color=#BB0000><size=60>1000TL</size></color> ÖDE"},
            {10 , "BAŞLANGIÇ NOKTASINA İLERLE\n BAŞLANGIÇ NOKTASI BONUSUNA EK OLARAK <color=#00BB00><size=60>2000TL</size></color> DAHA AL"},
            {11 , "EVİNİ KİRAYA VERDİN\n <color=#00BB00><size=60>1500TL</size></color> AL"}

        };

        var communityDataDict = new Dictionary<int, string>
        {

            {0 , "HAYVAN BARINAĞINDA TÜYLÜ DOSTLARIN BAĞIŞ İÇİN SANA MİNNETTAR OLACAK\n<color=#BB0000><size=60>500TL</size></color> ÖDE"},
            {1 , "MAHALLE OKULUNUN BAĞIŞ TOPLAMAK İÇİN DÜZENLEDİĞİ ARABA YIKAMA ETKİNLİĞİNE GİTTİN AMA CAMLARI AÇIK UNUTTUN\n <color=#BB0000><size=60>1000TL</size></color> ÖDE"},
            {2 , "KODESE GİR"},
            {3 , "KAN BAĞIŞI MERKEZİNDEKİ BEDAVA KURABİYELERİ KAÇIRMA\n<color=#00BB00><size=60>1000TL</size></color> AL"},
            {4 , "MAHALLENDEKİ İNSANLARIN KAYNAŞMASI İÇİN AÇIK HAVA PARTİSİ DÜZENLEDİN\nHER OYUNCUDAN <color=#00BB00><size=60>500TL</size></color> AL"},
            {5 , "BAŞLANGIÇ NOKTASINA İLERLE\n BAŞLANGIÇ NOKTASI BONUSUNA EK OLARAK <color=#00BB00><size=60>2000TL</size></color> DAHA AL"},
            {6 , "KOMŞUNUN ALIŞVERİŞ TORBALARINI TAŞIMASINA YARDIMCI OLDUN\n TEŞEKKÜR ETMEK İÇİN SANA YEMEK HAZIRLADI\n <color=#00BB00><size=60>2000TL</size></color> AL"},
            {7 , "SAHİP OLDUĞUN HER EV İÇİN 500TL ÖDE\n SAHİP OLDUĞUN HER OTEL İÇİN <color=#BB0000><size=60>1000TL</size></color> ÖDE"},
            {8 , "OKUL İÇİN YENİ BİR OYUN ALANI İNŞA ETMEYE YARDIM ETTİN\n KAYDIRAĞI TEST ETME SIRASI SENDE\n <color=#00BB00><size=60>1000TL</size></color> AL"},
            {9 , "HER HAFTA YAŞÇA BÜYÜK BİR KOMŞUNLA TAKILMAK İÇİN ZAMAN AYIRIYORSUN VE ONDAN HARİKA HİKAYELER DİNLEDİN\n <color=#00BB00><size=60>1000TL</size></color> AL"},
            {10 , "KUVVETLİ BİR FIRTINADAN SONRA KOMŞULARININ BAHÇELERİNİ TEMİZLEMERİNE YARDIMCI OLDUN\n <color=#00BB00><size=60>2000TL</size></color> AL"},
            {11 , "İLÇENİN YÜRÜYÜŞ YOLUNU TEMİZLEMEK İÇİN BİR GRUP ORGANİZE ETTİN\n <color=#BB0000><size=60>1000TL</size></color> ÖDE"},
            {12 , "BİR GÜNÜ ÇOCUK HASTANESİNDE ÇOCUKLARLA OYUN OYNAYARAK GEÇİRDİN\n <color=#00BB00><size=60>5000TL</size></color> AL"},
            {13 , "MAHALLENDEKİ OKUL İÇİN KERMES DÜZENLEDİN VE ÇOK PARA TOPLADIN\n <color=#00BB00><size=60>3000TL</size></color> AL"},
            {14 , "OKUL KERMESİNDEN BİRKAÇ KUTU KURABİYE SATIN ALDIN VE KOMŞULARINA DAĞITTIN\n <color=#BB0000><size=60>2000TL</size></color> ÖDE"}
        };
        
        return isChanceData ? chanceDataDict : communityDataDict;
    }
    public void HandleChanceOrCommunityTile(TileRuntimeData currentTile)
    {

        PlayerScript currentPlayer = GameManager.Instance.GetCurrentPlayer();

        if (currentTile.tileData.tileType is TileType.Chance)
        {
            int randomIndex = Random.Range(0, chanceCardEffects.Count);
            GameManager.Instance.SetupCardUI(cardText: chanceData[randomIndex], isChanceCard: true);
            CardData randomChanceCard = chanceCardEffects[randomIndex];
            randomChanceCard.Execute(currentPlayer);
        }
        else if (currentTile.tileData.tileType is TileType.Community)
        {
            int randomIndex = Random.Range(0, communityCardEffects.Count);
            GameManager.Instance.SetupCardUI(cardText: communityData[randomIndex], isChanceCard: false);
            CardData randomCommunityCard = communityCardEffects[randomIndex];
            randomCommunityCard.Execute(currentPlayer);
        }
        currentPlayer.RotatePlayer();
        currentPlayer.hasMadeDecision = true;
    }
    
    
    #endregion
} 