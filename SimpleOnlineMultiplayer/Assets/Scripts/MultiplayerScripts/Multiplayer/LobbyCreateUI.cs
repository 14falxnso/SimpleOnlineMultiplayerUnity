using TMPro;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private TestLObby testLobbyScript;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button createPublicButton;
    [SerializeField] private Button createPrivateButton;

    [SerializeField] private TMP_InputField lobbyNameInputField;

    private void Awake()
    {
        createPrivateButton.onClick.AddListener(() =>
        { testLobbyScript.CreateLobby(lobbyNameInputField.text, true); });
        createPublicButton.onClick.AddListener(() =>
        { testLobbyScript.CreateLobby(lobbyNameInputField.text, false); });
        closeButton.onClick.AddListener(() => { gameObject.SetActive(false); });
    }
}
