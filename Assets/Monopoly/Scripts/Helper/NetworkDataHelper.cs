using Mirror;
using static MonopolyNetworkManager;
public class NetworkDataHelper : NetworkBehaviour
{
    public static NetworkDataHelper Instance;

    // İşte aradığın o mermi gibi liste burada!
    public readonly SyncList<PlayerSessionData> finalLobbyPlayers = new SyncList<PlayerSessionData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
