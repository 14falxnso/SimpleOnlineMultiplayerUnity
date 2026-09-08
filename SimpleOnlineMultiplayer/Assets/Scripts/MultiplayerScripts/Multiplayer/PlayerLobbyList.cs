using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    public TestLObby testLobbyScript;

    private Player player;

    private void Start()
    {
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
        playerNameText.text = testLobbyScript.playerName;
    }

}
