
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerGame : NetworkBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int startingHealth = 100;
    [SerializeField] private int scorePerKill = 100;

    private NetworkList<NetworkPlayerStats> playerStats =
        new NetworkList<NetworkPlayerStats>();

    public NetworkList<NetworkPlayerStats> PlayerStatsList =>
        playerStats;

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback +=
                OnClientConnected;

            NetworkManager.Singleton.OnClientDisconnectCallback +=
                OnClientDisconnected;

            foreach (ulong clientId in
                     NetworkManager.Singleton.ConnectedClientsIds)
            {
                RegisterPlayer(clientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -=
            OnClientConnected;

        NetworkManager.Singleton.OnClientDisconnectCallback -=
            OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
            RegisterPlayer(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer)
            return;

        Debug.Log($"Client {clientId} heeft de game verlaten.");
    }

    private void RegisterPlayer(ulong clientId)
    {
        if (!IsServer || FindPlayerIndex(clientId) >= 0)
            return;

        NetworkPlayerStats stats = new NetworkPlayerStats
        {
            ClientId = clientId,
            Health = startingHealth,
            Kills = 0,
            Deaths = 0,
            Score = 0
        };

        playerStats.Add(stats);

        Debug.Log($"Stats geregistreerd voor client {clientId}.");
    }

    // =========================
    // HEALTH
    // =========================

    public void UpdateHealth(ulong clientId, float health)
    {
        if (!IsServer)
            return;

        int index = FindPlayerIndex(clientId);

        if (index < 0)
            return;

        NetworkPlayerStats stats = playerStats[index];
        stats.Health = Mathf.Clamp(health, 0f, startingHealth);

        playerStats[index] = stats;
    }

    // =========================
    // KILLS
    // =========================

    public void AddKill(ulong clientId)
    {
        if (!IsServer)
            return;

        int index = FindPlayerIndex(clientId);

        if (index < 0)
            return;

        NetworkPlayerStats stats = playerStats[index];

        stats.Kills++;
        stats.Score += scorePerKill;

        playerStats[index] = stats;

        Debug.Log(
            $"Client {clientId}: {stats.Kills} kills, " +
            $"{stats.Score} punten."
        );
    }

    // =========================
    // DEATHS
    // =========================

    public void AddDeath(ulong clientId)
    {
        if (!IsServer)
            return;

        int index = FindPlayerIndex(clientId);

        if (index < 0)
            return;

        NetworkPlayerStats stats = playerStats[index];
        stats.Deaths++;

        playerStats[index] = stats;

        Debug.Log($"Client {clientId}: {stats.Deaths} deaths.");
    }

    // =========================
    // STATS OPVRAGEN
    // =========================

    public NetworkPlayerStats GetPlayerStats(ulong clientId)
    {
        int index = FindPlayerIndex(clientId);

        if (index >= 0)
            return playerStats[index];

        return default;
    }

    private int FindPlayerIndex(ulong clientId)
    {
        for (int i = 0; i < playerStats.Count; i++)
        {
            if (playerStats[i].ClientId == clientId)
                return i;
        }

        return -1;
    }

    // =========================
    // MAP LADEN
    // =========================

    public void LoadMap(string sceneName)
    {
        if (!IsServer)
            return;

        if (NetworkManager.Singleton == null ||
            NetworkManager.Singleton.SceneManager == null)
        {
            Debug.LogError(
                "Netcode Scene Management is niet beschikbaar."
            );
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }
}

// =========================
// NETWORK PLAYER STATS
// =========================

public struct NetworkPlayerStats :
    INetworkSerializable,
    IEquatable<NetworkPlayerStats>
{
    public ulong ClientId;
    public float Health;
    public int Kills;
    public int Deaths;
    public int Score;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref Kills);
        serializer.SerializeValue(ref Deaths);
        serializer.SerializeValue(ref Score);
    }

    public bool Equals(NetworkPlayerStats other)
    {
        return ClientId == other.ClientId &&
               Health.Equals(other.Health) &&
               Kills == other.Kills &&
               Deaths == other.Deaths &&
               Score == other.Score;
    }
}