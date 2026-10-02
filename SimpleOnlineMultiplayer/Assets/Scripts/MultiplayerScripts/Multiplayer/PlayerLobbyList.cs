using TMPro;
using UnityEngine;

public class PlayerLobbyList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    public void SetPlayerName(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Player";

        playerNameText.text = playerName;
    }
}