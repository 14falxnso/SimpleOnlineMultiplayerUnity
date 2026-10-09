
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

    private float updateTimer;

    private void Start()
    {
        if (playerLobbyTemplate != null)
            playerLobbyTemplate.gameObject.SetActive(false);

        RefreshUI();
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        updateTimer -= Time.deltaTime;

        if (updateTimer <= 0f)
        {
            updateTimer = 1f;
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (testLobbyScript == null)
            return;

        Lobby lobby = testLobbyScript.GetLobby();

        if (lobby == null)
            return;

        // Lobby-informatie
        if (lobbyNameText != null)
            lobbyNameText.text = lobby.Name;

        if (lobbycodeText != null)
            lobbycodeText.text = "Code: " + lobby.LobbyCode;

        UpdatePlayerList(lobby);
    }

    private void UpdatePlayerList(Lobby lobby)
    {
        if (playerLobbyContainer == null ||
            playerLobbyTemplate == null)
            return;

        // Verwijder oude spelersrijen.
        foreach (Transform child in playerLobbyContainer)
        {
            if (child == playerLobbyTemplate)
                continue;

            Destroy(child.gameObject);
        }

        if (lobby.Players == null)
            return;

        // Maak een rij voor iedere speler in de actuele lobby.
        foreach (Player player in lobby.Players)
        {
            string playerName = "Player";

            if (player.Data != null &&
                player.Data.TryGetValue(
                    "PlayerName",
                    out PlayerDataObject nameData) &&
                !string.IsNullOrWhiteSpace(nameData.Value))
            {
                playerName = nameData.Value;
            }

            Transform row = Instantiate(
                playerLobbyTemplate,
                playerLobbyContainer
            );

            row.gameObject.SetActive(true);

            PlayerLobbyList playerRow =
                row.GetComponent<PlayerLobbyList>();

            if (playerRow != null)
            {
                playerRow.SetPlayerName(playerName);
            }
            else
            {
                Debug.LogWarning(
                    "PlayerLobbyTemplate mist het PlayerLobbyList-script."
                );
            }

            Debug.Log(
                $"Lobby speler: {playerName} | ID: {player.Id}"
            );
        }
    }
}
