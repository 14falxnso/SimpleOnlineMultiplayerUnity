using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private TestLObby testLobbyScript;

    [SerializeField] private TextMeshProUGUI lobbyNameTxt;
    [SerializeField] private TextMeshProUGUI lobbyCodeTxt;

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        if (testLobbyScript == null)
            return;

        Lobby lobby = testLobbyScript.GetLobby();

        if (lobby == null)
        {
            lobbyNameTxt.text = "LobbyName: -";
            lobbyCodeTxt.text = "Code: -";
            return;
        }

        lobbyNameTxt.text = "LobbyName: " + lobby.Name;
        lobbyCodeTxt.text = "Code: " + lobby.LobbyCode;
    }
}