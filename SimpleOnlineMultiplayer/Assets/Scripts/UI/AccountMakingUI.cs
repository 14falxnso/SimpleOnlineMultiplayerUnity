using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountMakingUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject makeAccountPanel;
    [SerializeField] private GameObject loggedInPanel;

    [Header("Game Panel")]
    [SerializeField] private GameObject maingamePanel;

    [Header("Make Account Panel")]
    [SerializeField] private TMP_InputField accountName;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button loginButton;

    [Header("Logged In Panel")]
    [SerializeField] private TextMeshProUGUI showNameText;
    [SerializeField] private Button logoutButton;

    [Header("Buttons")]
    [SerializeField] private Button startScreenButton;
    [SerializeField] private Button openGameButton;
    [SerializeField] private Button statsButton;

    [Header("UI Parts")]
    [SerializeField] private GameObject startScreenPanel;
    [SerializeField] private GameObject statsScreenPanel;

    [Header("Messages")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Multiplayer")]
    [SerializeField] private TestLObby testLobby;

    private void Start()
    {
        createAccountButton.onClick.AddListener(
            CreateAccount
        );

        if (loginButton != null)
        {
            loginButton.onClick.AddListener(
                Login
            );
        }

        logoutButton.onClick.AddListener(
            Logout
        );

        startScreenButton.onClick.AddListener(
            OpenStartScreen
        );

        openGameButton.onClick.AddListener(
            OpenGame
        );

        statsButton.onClick.AddListener(
            OpenStatsScreen
        );

        makeAccountPanel.SetActive(true);
        loggedInPanel.SetActive(false);

        startScreenPanel.SetActive(false);
        statsScreenPanel.SetActive(false);
        maingamePanel.SetActive(false);
    }

    // =========================================================
    // ACCOUNT AANMAKEN
    // =========================================================

    private void CreateAccount()
    {
        string username =
            accountName.text.Trim();

        string password =
            inputPassword.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowMessage(
                "Vul een accountnaam in."
            );

            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowMessage(
                "Vul een wachtwoord in."
            );

            return;
        }

        if (username.Length < 3)
        {
            ShowMessage(
                "Accountnaam moet minimaal 3 tekens zijn."
            );

            return;
        }

        if (password.Length < 4)
        {
            ShowMessage(
                "Wachtwoord moet minimaal 4 tekens zijn."
            );

            return;
        }

        bool success =
            AccountManager.Instance.Register(
                username,
                password
            );

        if (!success)
        {
            ShowMessage(
                "Deze accountnaam bestaat al."
            );

            return;
        }

        LoginSuccessful();
    }

    // =========================================================
    // INLOGGEN
    // =========================================================

    private void Login()
    {
        string username =
            accountName.text.Trim();

        string password =
            inputPassword.text;

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password))
        {
            ShowMessage(
                "Vul je accountnaam en wachtwoord in."
            );

            return;
        }

        bool success =
            AccountManager.Instance.Login(
                username,
                password
            );

        if (!success)
        {
            ShowMessage(
                "Accountnaam of wachtwoord is fout."
            );

            return;
        }

        LoginSuccessful();
    }

    // =========================================================
    // LOGIN GELUKT
    // =========================================================

    private void LoginSuccessful()
    {
        PlayerAccount account =
            AccountManager.Instance.CurrentAccount;

        if (account == null)
            return;

        // Accountnaam laten zien
        if (showNameText != null)
        {
            showNameText.text =
                account.username;
        }

        accountName.text = "";
        inputPassword.text = "";

        ShowMessage("");

        makeAccountPanel.SetActive(false);
        loggedInPanel.SetActive(true);

        /*
         * Unity Authentication wordt hier automatisch
         * gestart.
         *
         * De gebruiker hoeft dus NIET meer op
         * een aparte Authenticate-knop te drukken.
         */

        if (testLobby != null)
        {
            string multiplayerName =
                account.stats.lastUsedName;

            /*
             * Als de speler nog nooit een
             * multiplayernaam heeft gekozen,
             * gebruiken we tijdelijk de accountnaam.
             */

            if (string.IsNullOrWhiteSpace(
                multiplayerName))
            {
                multiplayerName =
                    account.username;
            }

            testLobby.Authenticate(
                account.username,
                multiplayerName
            );
        }

        OpenStartScreen();
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    private void Logout()
    {
        AccountManager.Instance.Logout();

        makeAccountPanel.SetActive(true);
        loggedInPanel.SetActive(false);

        startScreenPanel.SetActive(false);
        statsScreenPanel.SetActive(false);
        maingamePanel.SetActive(false);

        accountName.text = "";
        inputPassword.text = "";

        ShowMessage("");
    }

    // =========================================================
    // START SCREEN
    // =========================================================

    private void OpenStartScreen()
    {
        startScreenPanel.SetActive(true);
        statsScreenPanel.SetActive(false);
        maingamePanel.SetActive(false);
    }

    // =========================================================
    // GAME OPENEN
    // =========================================================

    private void OpenGame()
    {
        AccountManager.Instance.SaveCurrentAccount();

        startScreenPanel.SetActive(false);
        statsScreenPanel.SetActive(false);
        maingamePanel.SetActive(true);
    }

    // =========================================================
    // STATS
    // =========================================================

    private void OpenStatsScreen()
    {
        startScreenPanel.SetActive(false);
        statsScreenPanel.SetActive(true);
        maingamePanel.SetActive(false);

        UpdateStats();
    }

    private void UpdateStats()
    {
        PlayerAccount account =
            AccountManager.Instance.CurrentAccount;

        if (account == null)
            return;

        PlayerStats stats =
            account.stats;

        Debug.Log(
            "Wins: " + stats.wins
        );

        Debug.Log(
            "Games: " + stats.playedGames
        );

        Debug.Log(
            "Thrown Objects: " +
            stats.thrownObjects
        );

        Debug.Log(
            "Playtime: " +
            stats.playtime
        );
    }

    // =========================================================
    // MESSAGE
    // =========================================================

    private void ShowMessage(
        string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }
}
