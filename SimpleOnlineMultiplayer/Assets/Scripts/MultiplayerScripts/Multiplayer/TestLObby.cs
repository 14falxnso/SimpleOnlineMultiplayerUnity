using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class TestLObby : MonoBehaviour
{

    public Lobby hostLobby;
    public Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private float listLobbiesTimer;
    public string playerName;

    private const string KEY_START_GAME = "StartGame";

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    [Header("Zelfgemaakt var")]
    [SerializeField] private GameObject inLobbyUI;

    private int maxPlayers = 4;

    [SerializeField] private MPUIManager mpUIManager;
    [SerializeField] private TestRelay testRelayScript;

    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button authenticateButton;



    async void Start()
    {
        await UnityServices.InitializeAsync(); // request naar internet - geen antwoord > geen game


        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdate();
        HandlePeriodicListLobbies();
    }

    private void Awake()
    {
        authenticateButton.onClick.AddListener(() =>
        {
            Authenticate(playerNameInputField.text);
        });
    }
    public async void Authenticate (string playerName)
    {
        this.playerName = playerName;
        InitializationOptions options = new InitializationOptions();
        options.SetProfile(playerName);

        await UnityServices.InitializeAsync(options);

        AuthenticationService.Instance.SignedIn += () =>
        {
            HandlePeriodicListLobbies();
        };

        //await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void HandlePeriodicListLobbies()
{
    if (joinedLobby == null && AuthenticationService.Instance.IsSignedIn)
    {
        listLobbiesTimer -= Time.deltaTime;

        if (listLobbiesTimer <= 0f)
        {
            listLobbiesTimer = 3f;
            ListLobbies();
        }
    }
}

    public async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0f)
            {
                float heartBeatTimerMax = 15;
                    heartbeatTimer = heartBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id); // timer voor de lobby
            }

        }
    }

    public async void HandleLobbyPollForUpdate()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id); // timer voor de lobby

                if (joinedLobby.Data[KEY_START_GAME].Value != "0")
                {
                    string relayCode =
                        joinedLobby.Data[KEY_START_GAME].Value;

                    await testRelayScript.JoinRelay(relayCode);

                    joinedLobby = null;
                }
            }

        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        Player player = GetPlayer();
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            //Player = player,
            IsPrivate = isPrivate,
            Data = new Dictionary<string, DataObject>
            {
                {KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0")}
            }
        };


            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName,
                maxPlayers,
                options
            );

        hostLobby = lobby;
        joinedLobby = lobby;

        mpUIManager.InLobbyUIOnOff();

    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter> {
                new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
             
                }
            };
            

            QueryResponse queryReponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs {
            lobbyList = queryReponse.Results});
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyById(string lobbyId)
    {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId); // join de lobby
            joinedLobby = lobby;

            mpUIManager.InLobbyUIOnOff();
    }
    public async void JoinLobbyByCode(string lobbyCode)
    {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode); // join de lobby
            joinedLobby = lobby;

            mpUIManager.InLobbyUIOnOff();
    }

    public async void JoinLobby(Lobby lobby)
    {
        Player player = GetPlayer();

        joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
        {
            Player = player
        }); // join de lobby
        

        mpUIManager.InLobbyUIOnOff();
    }

    public async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();

            mpUIManager.InLobbyUIOnOff();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public Player GetPlayer()
    {
        return new Player // geef de player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"Playername", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
        };
    }

    public async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {


            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject> {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) } 
                }
            });
            joinedLobby = hostLobby;
        }catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public async void UpdatePlayerName(string newPlayername)
    {

            playerName = newPlayername;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id,
            AuthenticationService.Instance.PlayerId,
            new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>{
                    {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            });
    }


    public async void LeaveLobby()
    {
        try
        {
            testRelayScript.LeaveRelay();

            if (joinedLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(
                    joinedLobby.Id,
                    AuthenticationService.Instance.PlayerId
                );
            }

            joinedLobby = null;
            hostLobby = null;

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            testRelayScript.LeaveRelay();

            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

            joinedLobby = null;
            hostLobby = null;
            Debug.Log("deleted");
            mpUIManager.InLobbyUIOnOff();
            mpUIManager.LobbyUIOnOff();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void StartGame()
    {

            string relaycode = await testRelayScript.CreateRelay();

        Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { KEY_START_GAME , new DataObject(DataObject.VisibilityOptions.Member, relaycode)}
            }
        });

            joinedLobby = lobby;
        inLobbyUI.SetActive(false);
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }
}
