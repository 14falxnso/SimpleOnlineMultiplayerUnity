
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsScreenUIScript : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button returnButton;

    [Header("Panels")]
    [SerializeField] private GameObject statsScreenPanel;
    [SerializeField] private GameObject startScreenPanel;

    private void Start()
    {
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToStartScreen);
        }
    }

    public void LoadStatsUI()
    {
        if (AccountManager.Instance == null ||
            AccountManager.Instance.CurrentAccount == null)
        {
            statsText.text = "Geen account ingelogd.";
            return;
        }

        PlayerAccount account =
            AccountManager.Instance.CurrentAccount;

        PlayerStats stats = account.stats;

        if (stats == null)
        {
            statsText.text = "Geen statistieken beschikbaar.";
            return;
        }

        statsText.text =
            "Account: " + account.username + "\n\n" +
            "Wins: " + stats.wins + "\n" +
            "Games gespeeld: " + stats.playedGames + "\n" +
            "Objects gegooid: " + stats.thrownObjects + "\n" +
            "Speeltijd: " + stats.playtime;
    }

    private void ReturnToStartScreen()
    {
        statsScreenPanel.SetActive(false);
        startScreenPanel.SetActive(true);
    }
}
