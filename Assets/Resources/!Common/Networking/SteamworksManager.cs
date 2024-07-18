//using Netcode.Transports.Facepunch;
//using Steamworks;
//using Steamworks.Data;
//using System.Threading.Tasks;
//using Unity.Netcode;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class SteamworksManager : MonoBehaviour
//{
//    public static SteamworksManager Instance { get; private set; }

//    public NetworkPrefabsList NetworkPrefabs;
//    private FacepunchTransport _transport;

//    public Lobby Lobby;

//    private void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    private void Start()
//    {
//        SteamClient.RestartAppIfNecessary(480);
//        _transport = GetComponent<FacepunchTransport>();

//        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
//        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
//        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
//        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
//        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
//        //SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberLeave;
//        //SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
//        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
//        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
//        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
//    }

//    private void OnDestroy()
//    {
//        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
//        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
//        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
//        //SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
//        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
//        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
//        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
//    }

//    private void OnApplicationQuit() => LeaveLobby();

//    #region NetworkFlow
//    public async Task StartHost()
//    {
//        Lobby = (await SteamMatchmaking.CreateLobbyAsync(4)).Value;
//        NetworkManager.Singleton.StartHost();
//    }

//    public void StartClient(SteamId steamId)
//    {
//        _transport.targetSteamId = steamId.Value;
//        NetworkManager.Singleton.StartClient();
//    }

//    public void StartGame() => Lobby.SetGameServer(Lobby.Owner.Id);

//    public void ClientConnected(ulong id)
//    {
//        Debug.Log($"ClientId: {id}");
//        if (!NetworkManager.Singleton.IsHost) return;

//        var lobbySpawnPoints = GameObject.FindGameObjectsWithTag("LobbySpawnPoint");
//        GameObject player = Instantiate(NetworkPrefabs.PrefabList[0].Prefab, lobbySpawnPoints[id].transform);
//        player.transform.LookAt(Camera.main!.transform);
//        player.transform.rotation = Quaternion.Euler(new Vector3(0f, player.transform.rotation.eulerAngles.y, 0f));
//        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
//    }

//    public void ClientDisconnected(ulong id)
//    {
//        //if (NetworkManager.Singleton.IsHost) LeaveLobby();
//    }

//    public void LeaveLobby()
//    {
//        Lobby.Leave();
//        if (NetworkManager.Singleton) NetworkManager.Singleton.Shutdown();
//        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//        //UIManager.Open(UIManager.MainMenu);
//        //GameManager.Instance.GameStarted = false;
//    }
//    #endregion

//    #region SteamworksCallbacks
//    private void OnLobbyCreated(Result result, Lobby lobby)
//    {
//        if (result != Result.OK) return;
//        lobby.SetPublic();
//        lobby.SetJoinable(true);

//        SteamFriends.OpenGameInviteOverlay(lobby.Id);
//    }

//    private void OnLobbyInvite(Friend friend, Lobby lobby)
//    {

//    }

//    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
//    {
//        await lobby.Join();
//        Lobby = lobby;
//    }

//    private void OnLobbyEntered(Lobby lobby)
//    {
//        if (NetworkManager.Singleton.IsHost) return;
//        StartClient(Lobby.Owner.Id);
//        //UIManager.Open(UIManager.Lobby);
//    }

//    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
//    {
//        Debug.Log(friend.Id);
//    }

//    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
//    {
//        Debug.Log($"{friend.Id} is diconnected");
//    }

//    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId)
//    {
//        //UIManager.Lobby.SetActive(false);
//        //GameManager.Instance.SpawnPlayers();
//        //if (NetworkManager.Singleton.IsHost)
//        //{
//        //    GameManager.Instance.SpawnItems();
//        //    GameManager.Instance.SpawnEnemy();
//        //}
//        //GameManager.Instance.GameStarted = true;
//        lobby.SetJoinable(true);
//    }
//    #endregion
//}
