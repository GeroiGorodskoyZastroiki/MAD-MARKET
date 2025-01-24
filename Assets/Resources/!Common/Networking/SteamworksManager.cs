using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamworksManager : MonoBehaviour
{
    public static SteamworksManager Instance { get; private set; }

    private FacepunchTransport _transport;
    public NetworkPrefabsList NetworkPrefabs;
    
    public Lobby? Lobby;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        _transport = GetComponent<FacepunchTransport>();

        // SteamClient.Init(480, true);
        // if (!SteamClient.IsValid)
        //     Debug.Log("Steam client not valid");
    }

    private void Start()
    {
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    private void OnDestroy() => Shutdown();

    private void OnDisable() => Shutdown();

    private void OnApplicationQuit() => Shutdown();

    private void Shutdown()
    {
        if (Instance == this)
        {
            if (NetworkManager.Singleton == null) return;
            LeaveLobby();
            SteamClient.Shutdown();
        }
    }

    #region NetworkFlow
    public async Task StartHost(int maxMembers)
    {
        DestroyImmediate(GameManager.Instance.Player);
        NetworkManager.Singleton.StartHost();
        Lobby = (await SteamMatchmaking.CreateLobbyAsync(maxMembers)).Value;
        Debug.Log("HostStarted");
    }

    private void StartClient(SteamId steamId)
    {
        DestroyImmediate(GameManager.Instance.Player);
        _transport.targetSteamId = steamId.Value;
        NetworkManager.Singleton.StartClient();
        Debug.Log("ClientStarted");
    }

    public void StartLobby() => Lobby?.SetGameServer((SteamId)Lobby?.Owner.Id);

    public void LeaveLobby()
    {
        Lobby?.Leave();
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion

    #region SteamworksCallbacks
    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.Log("Can't create lobby");
            return;
        }
        Debug.Log("LobbyCreated");
        
        lobby.SetPublic();
        lobby.SetJoinable(true);

        SteamFriends.OpenGameInviteOverlay(lobby.Id);

        GameManager.Instance.PrepareGame();
    }

    private void OnLobbyInvite(Friend friend, Lobby lobby)
    {
        Debug.Log("Invited");
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        await lobby.Join();
        Lobby = lobby;
        Debug.Log("LobbyJoined");
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost) return;
        Debug.Log("EnteredLobby");
        StartClient(Lobby.Value.Owner.Id);
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log(friend.Name + " joined");
        Debug.Log(friend.Id);
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Id} is diconnected");
    }

    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId) //срабатывает когда хост запустил лобби (ни на что не влияет кроме названия кнопки в стиме?)
    {
        GameManager.Instance.StartGame();
    }
    #endregion
}
