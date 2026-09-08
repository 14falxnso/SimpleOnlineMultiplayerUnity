using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class InLobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbycodeText;

    public TestLObby testLobbyScript;
    private Lobby lobby;

    [SerializeField] private Transform playerLobbyContainer;
    [SerializeField] private Transform playerLobbyTemplate;

    private void Start()
    {
        UpdatePlayerList(new List<Player>());
        ShowLobbyName();

        
    }

    private void ShowLobbyName()
    {
        if (gameObject.activeSelf)
        {
            Lobby lobby = testLobbyScript.GetLobby();

            lobbyNameText.text = lobby.Name;
            lobbycodeText.text = "Code: " + lobby.LobbyCode;
        }
    }

    private void UpdatePlayerList(List<Player> playerList)
    {
        foreach (Transform child in playerLobbyContainer)
        {
            if (child == playerLobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Player player in playerList)
        {
            Transform playerLobbyTransform = Instantiate(playerLobbyTemplate, playerLobbyContainer);
            playerLobbyTransform.gameObject.SetActive(true);
            playerLobbyTransform.GetComponent<PlayerLobbyList>().SetPlayer(player);
        }
    }
}
