using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;

    public TestLObby testLobbyScript;

    private Lobby lobby;

    private void Start()
    {
    }
    private void Awake()
    {
        testLobbyScript = FindAnyObjectByType<TestLObby>();
        GetComponent<Button>().onClick.AddListener(() =>
        {
            testLobbyScript.JoinLobbyById(lobby.Id);
        });
    }
    public void SetLobby(Lobby lobby)
    {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
    }
    
}
