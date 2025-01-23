using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject PlayerPrefab;
    [HideInInspector] public GameObject Player;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void SpawnPlayer(bool value) =>
        Player = Instantiate(PlayerPrefab);

    private async void Start()
    {
        NetworkManager.Singleton.OnClientStopped += SpawnPlayer;
        NetworkManager.Singleton.OnServerStopped += SpawnPlayer;

        //LoadSettings();

        await SceneManager.LoadSceneAsync("Market");
        SpawnPlayer(true);
        
        //UIManager.Open(UIManager.MainMenu);
    }

    public void PrepareGame()
    {
        //создать карту
        //создать пропы
    }

    public void StartGame()
    {
        //расставить игроков
    }
}
