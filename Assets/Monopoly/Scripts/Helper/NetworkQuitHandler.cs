using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class NetworkQuitHandler : MonoBehaviour
{
    public void OnReturnToMainMenuButtonClicked()
    {
        // 1. Eğer Host isek (Hem Server hem Client aktifse)
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        // 2. Eğer sadece Client isek
        else if (NetworkClient.isConnected)
        {

            NetworkManager.singleton.StopClient();
        }
        // 3. Eğer hiçbir bağlantı yoksa (Sadece Menüde isek)
        else
        {
            // Ana menü sahnesinin adını buraya yaz
            SceneManager.LoadScene("MainMenu");
        }
    }
    public void OnQuitGameButtonClicked()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }


        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}