using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class InLobbyUI : MonoBehaviour
{
    [Header("Lobby Info")]
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbycodeText;

    [Header("References")]
    [SerializeField] private TestLObby testLobbyScript;

    [Header("Player List")]
    [SerializeField] private Transform playerLobbyContainer;
    [SerializeField] private Transform playerLobbyTemplate;

    private void Start()
    {
        if (playerLobbyTemplate != null)
            playerLobbyTemplate.gameObject.SetActive(false);

        ShowLobbyName();
        UpdatePlayerList();
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        UpdateLobbyData();
    }

    private float updateTimer;

    private void UpdateLobbyData()
    {
        updateTimer -= Time.deltaTime;

        if (updateTimer > 0f)
            return;

        updateTimer = 1f;

        ShowLobbyName();
        UpdatePlayerList();
    }

    // =========================================================
    // LOBBY INFO
    // =========================================================

    private void ShowLobbyName()
    {
        if (testLobbyScript == null)
            return;

        Lobby lobby = testLobbyScript.GetLobby();

        if (lobby == null)
            return;

        if (lobbyNameText != null)
            lobbyNameText.text = lobby.Name;

        if (lobbycodeText != null)
            lobbycodeText.text =
                "Code: " + lobby.LobbyCode;
    }

    // =========================================================
    // PLAYER LIST
    // =========================================================

    private void UpdatePlayerList()
    {
        if (testLobbyScript == null)
            return;

        Lobby lobby = testLobbyScript.GetLobby();

        if (lobby == null)
            return;

        foreach (Transform child in playerLobbyContainer)
        {
            if (child == playerLobbyTemplate)
                continue;

            Destroy(child.gameObject);
        }

        foreach (Unity.Services.Lobbies.Models.Player player
                 in lobby.Players)
        {
            string playerName = "Player";

            if (player.Data != null &&
                player.Data.ContainsKey("PlayerName"))
            {
                playerName =
                    player.Data["PlayerName"].Value;
            }

            Transform playerLobbyTransform =
                Instantiate(
                    playerLobbyTemplate,
                    playerLobbyContainer
                );

            playerLobbyTransform.gameObject.SetActive(true);

            PlayerLobbyList playerLobbyList =
                playerLobbyTransform
                    .GetComponent<PlayerLobbyList>();

            if (playerLobbyList != null)
            {
                playerLobbyList.SetPlayerName(
                    playerName
                );
            }
        }
    }
}