using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLObby : NetworkBehaviour
{
    public Lobby hostLobby;
    public Lobby joinedLobby;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private float listLobbiesTimer;

    // Naam van het lokale account
    public string accountName;

    // Naam die andere spelers zien
    public string playerName;

    private const string KEY_START_GAME = "StartGame";

    public event EventHandler<
        OnLobbyListChangedEventArgs
    > OnLobbyListChanged;

    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    [Header("Zelfgemaakt")]
    [SerializeField] private GameObject inLobbyUI;

    [SerializeField] private MPUIManager mpUIManager;
    [SerializeField] private TestRelay testRelayScript;

    private readonly int maxPlayers = 4;

    private bool isListingLobbies;
    private bool servicesInitialized = false;

    private NetworkList<PlayerData>
        playerDataNetworkList =
        new NetworkList<PlayerData>();

    private async void Start()
    {
        await System.Threading.Tasks.Task.Yield();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdate();
        HandlePeriodicListLobbies();
    }

    // =========================================================
    // AUTHENTICATE
    // =========================================================

    public async void Authenticate(
        string enteredAccountName,
        string enteredPlayerName)
    {
        if (string.IsNullOrWhiteSpace(
            enteredAccountName))
        {
            enteredAccountName = "Player";
        }

        if (string.IsNullOrWhiteSpace(
            enteredPlayerName))
        {
            enteredPlayerName =
                enteredAccountName;
        }

        accountName =
            enteredAccountName;

        playerName =
            enteredPlayerName;

        try
        {
            InitializationOptions options =
                new InitializationOptions();

            /*
             * De accountnaam wordt gebruikt als
             * Unity Services profile.
             */

            options.SetProfile(
                accountName
            );

            if (UnityServices.State ==
                ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync(
                    options
                );

                servicesInitialized = true;
            }
            else
            {
                servicesInitialized =
                    UnityServices.State ==
                    ServicesInitializationState.Initialized;
            }

            /*
             * Unity Authentication automatisch uitvoeren.
             */

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log(
                        "Signed in: " +
                        AuthenticationService.Instance.PlayerId
                    );
                };

                await AuthenticationService.Instance
                    .SignInAnonymouslyAsync();
            }

            Debug.Log(
                "Authentication successful."
            );

            Debug.Log(
                "Account: " +
                accountName
            );

            Debug.Log(
                "Multiplayer Name: " +
                playerName
            );

            /*
             * Als de NetworkPlayer al bestaat,
             * proberen we meteen de naam te sturen.
             */

            ApplyPlayerNameToNetworkPlayer();

            HandlePeriodicListLobbies();
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Authentication failed: " +
                e
            );
        }
    }

    // =========================================================
    // PLAYER NAME
    // =========================================================

    public void SetPlayerName(
        string newPlayerName)
    {
        if (string.IsNullOrWhiteSpace(
            newPlayerName))
        {
            return;
        }

        newPlayerName =
            newPlayerName.Trim();

        if (newPlayerName.Length > 20)
        {
            newPlayerName =
                newPlayerName.Substring(
                    0,
                    20
                );
        }

        playerName =
            newPlayerName;

        Debug.Log(
            "Multiplayer name changed to: " +
            playerName
        );

        /*
         * Als je al in een lobby zit,
         * wordt de naam daar aangepast.
         */

        UpdatePlayerName(
            playerName
        );

        /*
         * Ook de naam boven de player proberen
         * te veranderen.
         */

        ApplyPlayerNameToNetworkPlayer();
    }

    // =========================================================
    // PLAYER NAME NAAR NETWORK PLAYER
    // =========================================================

    private void ApplyPlayerNameToNetworkPlayer()
    {
        if (!IsSpawned)
            return;

        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsListening)
            return;

        SendPlayerNameToServerRpc(
            playerName
        );
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendPlayerNameToServerRpc(
        string newPlayerName,
        ServerRpcParams serverRpcParams = default)
    {
        ulong clientId =
            serverRpcParams
                .Receive
                .SenderClientId;

        if (string.IsNullOrWhiteSpace(
            newPlayerName))
        {
            newPlayerName = "Player";
        }

        int playerDataIndex =
            GetPlayerIndexFromClientId(
                clientId
            );

        if (playerDataIndex == -1)
        {
            playerDataNetworkList.Add(
                new PlayerData(
                    clientId,
                    newPlayerName
                )
            );
        }
        else
        {
            PlayerData playerData =
                playerDataNetworkList[
                    playerDataIndex
                ];

            playerData.playerName =
                newPlayerName;

            playerDataNetworkList[
                playerDataIndex
            ] = playerData;
        }

        SetPlayerNetworkName(
            clientId,
            newPlayerName
        );

        Debug.Log(
            $"Player name set | " +
            $"ClientId: {clientId} | " +
            $"Name: {newPlayerName}"
        );
    }

    // =========================================================
    // TEST PLAYER NAAM
    // =========================================================

    public void SetPlayerNetworkName(
        ulong clientId,
        string newPlayerName)
    {
        if (!IsServer)
            return;

        TestPlayer[] players =
            FindObjectsByType<TestPlayer>(
                FindObjectsSortMode.None
            );

        foreach (TestPlayer player in players)
        {
            if (player.OwnerClientId == clientId)
            {
                player.SetNetworkName(
                    newPlayerName
                );

                Debug.Log(
                    $"Network name updated for " +
                    $"ClientId {clientId}: " +
                    newPlayerName
                );

                break;
            }
        }
    }

    // =========================================================
    // NETWORK HOST
    // =========================================================

    public void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError(
                "NetworkManager.Singleton is missing."
            );

            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning(
                "NetworkManager is already listening."
            );

            return;
        }

        NetworkManager.Singleton
            .OnClientConnectedCallback +=
            NetworkManager_OnClientConnectedCallback;

        NetworkManager.Singleton
            .OnClientDisconnectCallback +=
            NetworkManager_OnClientDisconnectCallback;

        NetworkManager.Singleton.StartHost();

        Debug.Log(
            "Started Host."
        );
    }

    private void NetworkManager_OnClientConnectedCallback(
        ulong clientId)
    {
        if (!IsServer)
            return;

        int existingIndex =
            GetPlayerIndexFromClientId(
                clientId
            );

        if (existingIndex == -1)
        {
            playerDataNetworkList.Add(
                new PlayerData(
                    clientId,
                    ""
                )
            );
        }

        Debug.Log(
            "Client connected: " +
            clientId
        );
    }

    private void NetworkManager_OnClientDisconnectCallback(
        ulong clientId)
    {
        if (!IsServer)
            return;

        int index =
            GetPlayerIndexFromClientId(
                clientId
            );

        if (index != -1)
        {
            playerDataNetworkList.RemoveAt(
                index
            );
        }

        Debug.Log(
            "Client disconnected: " +
            clientId
        );
    }

    private int GetPlayerIndexFromClientId(
        ulong clientId)
    {
        for (
            int i = 0;
            i < playerDataNetworkList.Count;
            i++
        )
        {
            if (
                playerDataNetworkList[i].clientId ==
                clientId
            )
            {
                return i;
            }
        }

        return -1;
    }

    public List<PlayerData> GetPlayerDataList()
    {
        List<PlayerData> list =
            new List<PlayerData>();

        for (
            int i = 0;
            i < playerDataNetworkList.Count;
            i++
        )
        {
            list.Add(
                playerDataNetworkList[i]
            );
        }

        return list;
    }

    // =========================================================
    // LOBBY HEARTBEAT
    // =========================================================

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby == null)
            return;

        heartbeatTimer -=
            Time.deltaTime;

        if (heartbeatTimer <= 0f)
        {
            heartbeatTimer = 15f;

            try
            {
                await LobbyService.Instance
                    .SendHeartbeatPingAsync(
                        hostLobby.Id
                    );
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(
                    "Heartbeat failed: " +
                    e
                );
            }
        }
    }

    // =========================================================
    // LOBBY UPDATE
    // =========================================================

    private async void HandleLobbyPollForUpdate()
    {
        if (joinedLobby == null)
            return;

        lobbyUpdateTimer -=
            Time.deltaTime;

        if (lobbyUpdateTimer <= 0f)
        {
            lobbyUpdateTimer = 1.1f;

            try
            {
                joinedLobby =
                    await LobbyService.Instance
                        .GetLobbyAsync(
                            joinedLobby.Id
                        );

                if (
                    joinedLobby.Data.ContainsKey(
                        KEY_START_GAME
                    )
                )
                {
                    string relayCode =
                        joinedLobby.Data[
                            KEY_START_GAME
                        ].Value;

                    if (relayCode != "0")
                    {
                        await testRelayScript
                            .JoinRelay(
                                relayCode
                            );

                        joinedLobby = null;
                    }
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(
                    "Lobby update failed: " +
                    e
                );
            }
        }
    }

    // =========================================================
    // LOBBY LIST
    // =========================================================

    private void HandlePeriodicListLobbies()
    {
        if (joinedLobby != null)
            return;

        if (!AuthenticationService.Instance.IsSignedIn)
            return;

        listLobbiesTimer -=
            Time.deltaTime;

        if (listLobbiesTimer <= 0f)
        {
            listLobbiesTimer = 3f;

            ListLobbies();
        }
    }

    public async void ListLobbies()
    {
        if (isListingLobbies)
            return;

        if (!AuthenticationService.Instance.IsSignedIn)
            return;

        isListingLobbies = true;

        try
        {
            QueryLobbiesOptions queryLobbiesOptions =
                new QueryLobbiesOptions
                {
                    Filters =
                        new List<QueryFilter>
                        {
                            new QueryFilter(
                                QueryFilter.FieldOptions
                                    .AvailableSlots,
                                "0",
                                QueryFilter.OpOptions.GT
                            )
                        }
                };

            QueryResponse queryResponse =
                await LobbyService.Instance
                    .QueryLobbiesAsync(
                        queryLobbiesOptions
                    );

            OnLobbyListChanged?.Invoke(
                this,
                new OnLobbyListChangedEventArgs
                {
                    lobbyList =
                        queryResponse.Results
                }
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "List lobbies failed: " +
                e
            );
        }
        finally
        {
            isListingLobbies = false;
        }
    }

    // =========================================================
    // CREATE LOBBY
    // =========================================================

    public async void CreateLobby(
        string lobbyName,
        bool isPrivate)
    {
        try
        {
            Player player =
                GetPlayer();

            CreateLobbyOptions options =
                new CreateLobbyOptions
                {
                    Player = player,
                    IsPrivate = isPrivate,

                    Data =
                        new Dictionary<
                            string,
                            DataObject>
                        {
                            {
                                KEY_START_GAME,
                                new DataObject(
                                    DataObject
                                        .VisibilityOptions
                                        .Member,
                                    "0"
                                )
                            }
                        }
                };

            Lobby lobby =
                await LobbyService.Instance
                    .CreateLobbyAsync(
                        lobbyName,
                        maxPlayers,
                        options
                    );

            hostLobby = lobby;
            joinedLobby = lobby;

            if (mpUIManager != null)
            {
                mpUIManager.InLobbyUIOnOff();
            }

            Debug.Log(
                "Created lobby: " +
                lobby.Id
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Create lobby failed: " +
                e
            );
        }
    }

    // =========================================================
    // JOIN LOBBY BY ID
    // =========================================================

public async void JoinLobbyById(string lobbyId)
    {
        try
        {
            Player player = GetPlayer();

            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(
                lobbyId,
                new JoinLobbyByIdOptions
                {
                    Player = player
                }
            );

            Debug.Log("Joined lobby as: " + playerName);

            if (mpUIManager != null)
            {
                mpUIManager.InLobbyUIOnOff();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Join lobby by ID failed: " + e);
        }
    }


    // =========================================================
    // JOIN LOBBY BY CODE
    // =========================================================

public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            Player player = GetPlayer();

            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(
                lobbyCode,
                new JoinLobbyByCodeOptions
                {
                    Player = player
                }
            );

            Debug.Log("Joined lobby as: " + playerName);

            if (mpUIManager != null)
            {
                mpUIManager.InLobbyUIOnOff();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Join lobby by code failed: " + e);
        }
    }


    // =========================================================
    // JOIN LOBBY
    // =========================================================

    public async void JoinLobby(
        Lobby lobby)
    {
        if (lobby == null)
            return;

        try
        {
            Player player =
                GetPlayer();

            joinedLobby =
                await LobbyService.Instance
                    .JoinLobbyByIdAsync(
                        lobby.Id,
                        new JoinLobbyByIdOptions
                        {
                            Player = player
                        }
                    );

            if (mpUIManager != null)
            {
                mpUIManager.InLobbyUIOnOff();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Join lobby failed: " +
                e
            );
        }
    }

    // =========================================================
    // QUICK JOIN
    // =========================================================

    public async void QuickJoinLobby()
    {
        try
        {
            joinedLobby =
                await LobbyService.Instance
                    .QuickJoinLobbyAsync(
                        new QuickJoinLobbyOptions
                        {
                            Player =
                                GetPlayer()
                        }
                    );

            if (mpUIManager != null)
            {
                mpUIManager.InLobbyUIOnOff();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Quick Join failed: " +
                e
            );
        }
    }

    // =========================================================
    // GET PLAYER
    // =========================================================

    public Player GetPlayer()
    {
        string nameToSend = playerName;

        if (string.IsNullOrWhiteSpace(nameToSend))
        {
            nameToSend = "Player";
        }

        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
        {
            {
                "PlayerName",
                new PlayerDataObject(
                    PlayerDataObject.VisibilityOptions.Member,
                    nameToSend
                )
            }
        }
        };
    }

    // =========================================================
    // UPDATE PLAYER NAME
    // =========================================================

    public async void UpdatePlayerName(
        string newPlayerName)
    {
        if (string.IsNullOrWhiteSpace(
            newPlayerName))
        {
            return;
        }

        playerName =
            newPlayerName.Trim();

        if (joinedLobby == null)
            return;

        try
        {
            await LobbyService.Instance
                .UpdatePlayerAsync(
                    joinedLobby.Id,
                    AuthenticationService.Instance
                        .PlayerId,
                    new UpdatePlayerOptions
                    {
                        Data =
                            new Dictionary<
                                string,
                                PlayerDataObject>
                            {
                                {
                                    "PlayerName",
                                    new PlayerDataObject(
                                        PlayerDataObject
                                            .VisibilityOptions
                                            .Member,
                                        playerName
                                    )
                                }
                            }
                    }
                );

            Debug.Log(
                "Lobby player name updated: " +
                playerName
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Update player name failed: " +
                e
            );
        }
    }

    // =========================================================
    // GAME MODE
    // =========================================================

    public async void UpdateLobbyGameMode(
        string gameMode)
    {
        if (hostLobby == null)
            return;

        try
        {
            hostLobby =
                await LobbyService.Instance
                    .UpdateLobbyAsync(
                        hostLobby.Id,
                        new UpdateLobbyOptions
                        {
                            Data =
                                new Dictionary<
                                    string,
                                    DataObject>
                                {
                                    {
                                        "GameMode",
                                        new DataObject(
                                            DataObject
                                                .VisibilityOptions
                                                .Public,
                                            gameMode
                                        )
                                    }
                                }
                        }
                    );

            joinedLobby =
                hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Update game mode failed: " +
                e
            );
        }
    }

    // =========================================================
    // START GAME
    // =========================================================

    public async void StartGame()
    {
        if (joinedLobby == null)
            return;

        if (testRelayScript == null)
        {
            Debug.LogError(
                "TestRelay script is missing."
            );

            return;
        }

        try
        {
            string relayCode =
                await testRelayScript
                    .CreateRelay();

            if (string.IsNullOrEmpty(
                relayCode))
            {
                Debug.LogError(
                    "Relay code was null."
                );

                return;
            }

            Lobby lobby =
                await LobbyService.Instance
                    .UpdateLobbyAsync(
                        joinedLobby.Id,
                        new UpdateLobbyOptions
                        {
                            Data =
                                new Dictionary<
                                    string,
                                    DataObject>
                                {
                                    {
                                        KEY_START_GAME,
                                        new DataObject(
                                            DataObject
                                                .VisibilityOptions
                                                .Member,
                                            relayCode
                                        )
                                    }
                                }
                        }
                    );

            joinedLobby =
                lobby;

            if (inLobbyUI != null)
            {
                inLobbyUI.SetActive(false);
            }

            Debug.Log(
                "Game started. Relay code: " +
                relayCode
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Start game failed: " +
                e
            );
        }
    }

    // =========================================================
    // LEAVE LOBBY
    // =========================================================

    public async void LeaveLobby()
    {
        try
        {
            if (testRelayScript != null)
            {
                testRelayScript.LeaveRelay();
            }

            if (joinedLobby != null)
            {
                await LobbyService.Instance
                    .RemovePlayerAsync(
                        joinedLobby.Id,
                        AuthenticationService.Instance
                            .PlayerId
                    );
            }

            joinedLobby = null;
            hostLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Leave lobby failed: " +
                e
            );
        }
    }

    // =========================================================
    // KICK
    // =========================================================

    public async void KickPlayer()
    {
        if (joinedLobby == null)
            return;

        if (joinedLobby.Players.Count < 2)
            return;

        try
        {
            await LobbyService.Instance
                .RemovePlayerAsync(
                    joinedLobby.Id,
                    joinedLobby.Players[1].Id
                );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Kick failed: " +
                e
            );
        }
    }

    // =========================================================
    // HOST MIGRATION
    // =========================================================

    public async void MigrateLobbyHost()
    {
        if (joinedLobby == null)
            return;

        if (joinedLobby.Players.Count < 2)
            return;

        try
        {
            string newHostId =
                joinedLobby.Players[1].Id;

            hostLobby =
                await LobbyService.Instance
                    .UpdateLobbyAsync(
                        joinedLobby.Id,
                        new UpdateLobbyOptions
                        {
                            HostId =
                                newHostId
                        }
                    );

            joinedLobby =
                hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Host migration failed: " +
                e
            );
        }
    }

    // =========================================================
    // DELETE LOBBY
    // =========================================================

    public async void DeleteLobby()
    {
        if (joinedLobby == null)
            return;

        try
        {
            if (testRelayScript != null)
            {
                testRelayScript.LeaveRelay();
            }

            await LobbyService.Instance
                .DeleteLobbyAsync(
                    joinedLobby.Id
                );

            joinedLobby = null;
            hostLobby = null;

            if (mpUIManager != null)
            {
                mpUIManager.InLobbyUIOnOff();
                mpUIManager.LobbyUIOnOff();
            }

            Debug.Log(
                "Lobby deleted."
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(
                "Delete lobby failed: " +
                e
            );
        }
    }

    // =========================================================
    // GET LOBBY
    // =========================================================

    public Lobby GetLobby()
    {
        return joinedLobby;
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton
            .OnClientConnectedCallback -=
            NetworkManager_OnClientConnectedCallback;

        NetworkManager.Singleton
            .OnClientDisconnectCallback -=
            NetworkManager_OnClientDisconnectCallback;
    }
}
