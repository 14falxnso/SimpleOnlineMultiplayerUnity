using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MPRoundSystem : NetworkBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private float roundDuration = 300f;
    [SerializeField] private int roundsToWin = 5;
    [SerializeField] private int firstToNumber = 3;

    public NetworkVariable<int> CurrentRound =
        new NetworkVariable<int>(1);

    public NetworkVariable<float> CurrentTime =
        new NetworkVariable<float>();

    public NetworkVariable<bool> RoundActive =
        new NetworkVariable<bool>(false);

    private Dictionary<ulong, int> playerScores =
        new Dictionary<ulong, int>();

    private Dictionary<ulong, string> playerNames =
        new Dictionary<ulong, string>();

    private HashSet<ulong> readyPlayers =
        new HashSet<ulong>();

    [SerializeField] private GameObject inGameUI;

    [Header("Link Scripts")]
    [SerializeField] private MPUIManager mpUIManager;

    [Header("Round Info")]
    [SerializeField] private TextMeshProUGUI roundTxt;
    [SerializeField] private TextMeshProUGUI roundTimeTxt;

    [Header("Finish Info")]
    [SerializeField] private GameObject winnerScreen;
    [SerializeField] private TextMeshProUGUI winnerTxt;

    [SerializeField] private GameObject joinBackButton;
    [SerializeField] private GameObject leaveGameButton;
    [SerializeField] private GameObject continueOnButton;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback +=
                HandleClientConnected;
        }

        if (continueOnButton != null)
        {
            Button button =
                continueOnButton.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.AddListener(
                    OnContinueButtonPressed
                );
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null &&
            IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -=
                HandleClientConnected;
        }

        if (continueOnButton != null)
        {
            Button button =
                continueOnButton.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.RemoveListener(
                    OnContinueButtonPressed
                );
            }
        }
    }

    private void OnContinueButtonPressed()
    {
        SetReadyServerRpc();

        if (continueOnButton != null)
            continueOnButton.SetActive(false);
    }

    public void RegisterPlayerName(
        ulong clientId,
        string playerName)
    {
        playerNames[clientId] = playerName;

        Debug.Log(
            $"Registered: {clientId} = {playerName}"
        );
    }

    private string GetPlayerName(
        ulong clientId)
    {
        if (playerNames.TryGetValue(
            clientId,
            out string name))
        {
            return name;
        }

        return $"Player {clientId}";
    }

    private void HandleClientConnected(
        ulong clientId)
    {
        if (!playerScores.ContainsKey(clientId))
        {
            playerScores.Add(clientId, 0);
        }

        if (NetworkManager.Singleton.ConnectedClients.Count >= 2 &&
            !RoundActive.Value &&
            CurrentRound.Value == 1)
        {
            StartRound();
        }
    }

    private void Update()
    {
        // UI voor iedereen
        if (RoundActive.Value)
        {
            if (inGameUI != null &&
                inGameUI.activeSelf)
            {
                if (roundTxt != null)
                {
                    roundTxt.text =
                        "Round: " +
                        CurrentRound.Value;
                }

                if (roundTimeTxt != null)
                {
                    roundTimeTxt.text =
                        "Time left: " +
                        Mathf.CeilToInt(
                            CurrentTime.Value
                        );
                }
            }
        }

        // Alleen server bestuurt de game-logica
        if (!IsServer ||
            !RoundActive.Value)
        {
            return;
        }

        CurrentTime.Value -=
            Time.deltaTime;

        if (CurrentTime.Value <= 0f)
        {
            CurrentTime.Value = 0f;
            EndRound(null);
            return;
        }

        CheckPlayersAlive();
    }

    private void StartRound()
    {
        if (!IsServer)
            return;

        RoundActive.Value = true;
        CurrentTime.Value = roundDuration;

        readyPlayers.Clear();

        ShowGameUIClientRpc();

        Debug.Log(
            $"Round {CurrentRound.Value} started."
        );
    }

    private void EndRound(
        PlayerHealth winner)
    {
        if (!IsServer ||
            !RoundActive.Value)
        {
            return;
        }

        RoundActive.Value = false;

        if (winner != null)
        {
            ulong winnerId =
                winner.OwnerClientId;

            if (!playerScores.ContainsKey(winnerId))
            {
                playerScores[winnerId] = 0;
            }

            playerScores[winnerId]++;

            Debug.Log(
                $"Player {winnerId} won round. " +
                $"Score: {playerScores[winnerId]}"
            );

            if (playerScores[winnerId] >=
                roundsToWin)
            {
                EndMatch(winnerId);
                return;
            }

            string winnerName =
                GetPlayerName(winnerId);

            ShowRoundWinnerClientRpc(
                winnerName
            );
        }
        else
        {
            ShowRoundWinnerClientRpc(
                "Gelijkspel"
            );
        }
    }

    private void StartNextRound()
    {
        if (!IsServer)
            return;

        readyPlayers.Clear();

        CurrentRound.Value++;

        RespawnAllPlayers();

        StartRound();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(
        ServerRpcParams rpcParams = default)
    {
        if (!IsServer)
            return;

        ulong clientId =
            rpcParams.Receive.SenderClientId;

        readyPlayers.Add(clientId);

        Debug.Log(
            $"Player {clientId} is ready. " +
            $"{readyPlayers.Count}/" +
            $"{NetworkManager.Singleton.ConnectedClients.Count}"
        );

        if (readyPlayers.Count >=
            NetworkManager.Singleton.ConnectedClients.Count)
        {
            StartNextRound();
        }
    }

    private void CheckPlayersAlive()
    {
        PlayerHealth[] players =
            FindObjectsByType<PlayerHealth>(
                FindObjectsSortMode.None
            );

        int alivePlayers = 0;
        PlayerHealth lastAlive = null;

        foreach (PlayerHealth player in players)
        {
            if (player.health > 0f)
            {
                alivePlayers++;
                lastAlive = player;
            }
        }

        if (alivePlayers <= 1)
        {
            EndRound(lastAlive);
        }
    }

    private void RespawnAllPlayers()
    {
        if (!IsServer)
            return;

        PlayerHealth[] players =
            FindObjectsByType<PlayerHealth>(
                FindObjectsSortMode.None
            );

        foreach (PlayerHealth player in players)
        {
            player.ResetHealth();

            player.transform.position =
                GetSpawnPosition();
        }
    }

    private Vector3 GetSpawnPosition()
    {
        // Later kun je hier echte spawnpoints gebruiken.
        return Vector3.zero;
    }

    private void EndMatch(
        ulong winnerId)
    {
        if (!IsServer)
            return;

        string winnerName =
            GetPlayerName(winnerId);

        ShowMatchWinnerClientRpc(
            winnerName,
            true
        );
    }

    // =========================================================
    // UI CLIENT RPCS
    // =========================================================

    [ClientRpc]
    private void ShowGameUIClientRpc()
    {
        if (winnerScreen != null)
            winnerScreen.SetActive(false);

        if (inGameUI != null)
            inGameUI.SetActive(true);

        if (continueOnButton != null)
            continueOnButton.SetActive(false);

        if (leaveGameButton != null)
            leaveGameButton.SetActive(false);

        if (joinBackButton != null)
            joinBackButton.SetActive(false);
    }

    [ClientRpc]
    private void ShowRoundWinnerClientRpc(
        string winnerName)
    {
        if (inGameUI != null)
            inGameUI.SetActive(false);

        if (winnerScreen != null)
            winnerScreen.SetActive(true);

        if (winnerTxt != null)
        {
            winnerTxt.text =
                winnerName +
                " wins this round!";
        }

        if (continueOnButton != null)
            continueOnButton.SetActive(true);

        if (leaveGameButton != null)
            leaveGameButton.SetActive(false);

        if (joinBackButton != null)
            joinBackButton.SetActive(false);
    }

    [ClientRpc]
    private void ShowMatchWinnerClientRpc(
        string winnerName,
        bool isFinalRound)
    {
        if (inGameUI != null)
            inGameUI.SetActive(false);

        if (winnerScreen != null)
            winnerScreen.SetActive(true);

        if (winnerTxt != null)
        {
            winnerTxt.text =
                winnerName +
                " wint the game!";
        }

        if (continueOnButton != null)
            continueOnButton.SetActive(false);

        if (leaveGameButton != null)
            leaveGameButton.SetActive(true);

        if (joinBackButton != null)
            joinBackButton.SetActive(isFinalRound);
    }
}