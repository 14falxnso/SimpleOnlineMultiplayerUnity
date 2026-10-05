using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Lobby")]
    [SerializeField] private TestLObby testLobbyScript;

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TMP_InputField joinCodeInputField;

    [Header("Multiplayer Name")]
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Button savePlayerNameButton;
    [SerializeField] private TextMeshProUGUI playerNameMessageText;

    [Header("Lobby Create")]
    [SerializeField] private LobbyCreateUI lobbyCreateUI;

    [Header("Lobby List")]
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Awake()
    {
        /*
         * Deze buttons worden automatisch gekoppeld.
         * Je hoeft dus niets in Button -> OnClick() te zetten.
         */

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() =>
            {
                // Stuurt naar main menu
            });
        }

        if (createLobbyButton != null)
        {
            createLobbyButton.onClick.AddListener(() =>
            {
                if (lobbyCreateUI != null)
                {
                    lobbyCreateUI.gameObject.SetActive(true);
                }
            });
        }

        if (quickJoinButton != null)
        {
            quickJoinButton.onClick.AddListener(() =>
            {
                SavePlayerName();

                testLobbyScript.QuickJoinLobby();
            });
        }

        if (joinLobbyButton != null)
        {
            joinLobbyButton.onClick.AddListener(() =>
            {
                SavePlayerName();

                testLobbyScript.JoinLobbyByCode(
                    joinCodeInputField.text
                );
            });
        }

        if (savePlayerNameButton != null)
        {
            savePlayerNameButton.onClick.AddListener(
                SavePlayerName
            );
        }

        if (lobbyTemplate != null)
        {
            lobbyTemplate.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (testLobbyScript != null)
        {
            testLobbyScript.OnLobbyListChanged +=
                TestLobbyScript_OnLobbyListChanged;
        }

        LoadPlayerName();

        UpdateLobbyList(
            new List<Lobby>()
        );
    }

    // =========================================================
    // PLAYER NAME LADEN
    // =========================================================

    private void LoadPlayerName()
    {
        if (testLobbyScript == null)
            return;

        string currentName =
            testLobbyScript.playerName;

        /*
         * Als TestLObby nog geen naam heeft,
         * kijken we naar het lokale account.
         */

        if (string.IsNullOrWhiteSpace(currentName))
        {
            if (AccountManager.Instance != null &&
                AccountManager.Instance.CurrentAccount != null)
            {
                currentName =
                    AccountManager.Instance
                        .CurrentAccount
                        .stats
                        .lastUsedName;
            }
        }

        /*
         * Als er nog helemaal geen naam is,
         * gebruiken we Player.
         */

        if (string.IsNullOrWhiteSpace(currentName))
        {
            currentName = "Player";
        }

        if (playerNameInputField != null)
        {
            playerNameInputField.text =
                currentName;
        }
    }

    // =========================================================
    // PLAYER NAME OPSLAAN
    // =========================================================

    private void SavePlayerName()
    {
        if (testLobbyScript == null)
            return;

        if (playerNameInputField == null)
            return;

        string newName =
            playerNameInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(newName))
        {
            ShowPlayerNameMessage(
                "Vul een multiplayernaam in."
            );

            return;
        }

        if (newName.Length < 3)
        {
            ShowPlayerNameMessage(
                "Naam moet minimaal 3 tekens zijn."
            );

            return;
        }

        if (newName.Length > 20)
        {
            ShowPlayerNameMessage(
                "Naam mag maximaal 20 tekens zijn."
            );

            return;
        }

        /*
         * Naam naar TestLObby sturen.
         */

        testLobbyScript.SetPlayerName(
            newName
        );

        /*
         * Naam lokaal opslaan bij het account.
         */

        if (AccountManager.Instance != null &&
            AccountManager.Instance.CurrentAccount != null)
        {
            AccountManager.Instance
                .CurrentAccount
                .stats
                .lastUsedName = newName;

            AccountManager.Instance.SaveCurrentAccount();
        }

        ShowPlayerNameMessage(
            "Naam opgeslagen!"
        );
    }

    private void ShowPlayerNameMessage(
        string message)
    {
        if (playerNameMessageText != null)
        {
            playerNameMessageText.text =
                message;
        }
    }

    // =========================================================
    // LOBBY LIST
    // =========================================================

    private void TestLobbyScript_OnLobbyListChanged(
        object sender,
        TestLObby.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(
            e.lobbyList
        );
    }

    private void UpdateLobbyList(
        List<Lobby> lobbyList)
    {
        if (lobbyContainer == null ||
            lobbyTemplate == null)
        {
            return;
        }

        foreach (Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate)
                continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyTransform =
                Instantiate(
                    lobbyTemplate,
                    lobbyContainer
                );

            lobbyTransform.gameObject.SetActive(true);

            LobbyListSingleUI lobbyListSingleUI =
                lobbyTransform.GetComponent<
                    LobbyListSingleUI
                >();

            if (lobbyListSingleUI == null)
            {
                Debug.LogError(
                    "LobbyListSingleUI ontbreekt op het lobbyTemplate!"
                );

                continue;
            }

            lobbyListSingleUI.SetLobby(
                lobby
            );
        }
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (testLobbyScript != null)
        {
            testLobbyScript.OnLobbyListChanged -=
                TestLobbyScript_OnLobbyListChanged;
        }
    }
}