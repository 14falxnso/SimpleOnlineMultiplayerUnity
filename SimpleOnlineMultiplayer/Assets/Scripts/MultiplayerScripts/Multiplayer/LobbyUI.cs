using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TestLObby testLobbyScript;

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TMP_InputField joinCodeInputField;

    [SerializeField] private LobbyCreateUI lobbyCreateUI;

    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            // Stuurt naar main menu
        });

        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyCreateUI.gameObject.SetActive(true);
        });

        quickJoinButton.onClick.AddListener(() =>
        {
            testLobbyScript.QuickJoinLobby();
        });

        joinLobbyButton.onClick.AddListener(() =>
        {
            testLobbyScript.JoinLobbyByCode(joinCodeInputField.text);
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        testLobbyScript.OnLobbyListChanged += TestLobbyScript_OnLobbyListChanged;

        UpdateLobbyList(new List<Lobby>());
    }

    private void TestLobbyScript_OnLobbyListChanged(
        object sender,
        TestLObby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate)
                continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform =
                Instantiate(lobbyTemplate, lobbyContainer);

            lobbyTransform.gameObject.SetActive(true);

            LobbyListSingleUI lobbyListSingleUI =
                lobbyTransform.GetComponent<LobbyListSingleUI>();

            if (lobbyListSingleUI == null)
            {
                Debug.LogError(
                    "LobbyListSingleUI ontbreekt op het lobbyTemplate!"
                );

                continue;
            }

            lobbyListSingleUI.SetLobby(lobby);
        }
    }

    private void OnDestroy()
    {
        if (testLobbyScript != null)
        {
            testLobbyScript.OnLobbyListChanged -=
                TestLobbyScript_OnLobbyListChanged;
        }
    }
}