
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance;

    private AccountDatabase database;
    private PlayerAccount currentAccount;
    private string filePath;

    public PlayerAccount CurrentAccount => currentAccount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        string projectFolder =
            Directory.GetParent(Application.dataPath).FullName;

        string saveFolder =
            Path.Combine(projectFolder, "SavedData");

        Directory.CreateDirectory(saveFolder);

        filePath = Path.Combine(saveFolder, "accounts.json");

        string oldFilePath = Path.Combine(
            Application.persistentDataPath,
            "accounts.json"
        );

        if (!File.Exists(filePath) && File.Exists(oldFilePath))
        {
            File.Copy(oldFilePath, filePath);
        }

        LoadAccounts();
    }

    private void LoadAccounts()
    {
        if (!File.Exists(filePath))
        {
            database = new AccountDatabase();
            SaveAccounts();
            return;
        }

        string json = File.ReadAllText(filePath);
        database = JsonUtility.FromJson<AccountDatabase>(json);

        if (database == null)
            database = new AccountDatabase();

        if (database.accounts == null)
            database.accounts = new List<PlayerAccount>();
    }

    private void SaveAccounts()
    {
        string json = JsonUtility.ToJson(database, true);
        File.WriteAllText(filePath, json);
    }

    // Alle accountnamen ophalen voor de dropdown.
    public List<string> GetAccountNames()
    {
        List<string> names = new List<string>();

        foreach (PlayerAccount account in database.accounts)
        {
            names.Add(account.username);
        }

        return names;
    }

    // Normaal account aanmaken.
    public bool Register(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
            return false;

        foreach (PlayerAccount account in database.accounts)
        {
            if (account.username == username)
                return false;
        }

        PlayerAccount newAccount =
            new PlayerAccount(username, password);

        database.accounts.Add(newAccount);
        SaveAccounts();

        currentAccount = newAccount;
        return true;
    }

    // Normaal inloggen met wachtwoord.
    public bool Login(string username, string password)
    {
        foreach (PlayerAccount account in database.accounts)
        {
            if (account.username == username &&
                account.password == password)
            {
                currentAccount = account;
                return true;
            }
        }

        return false;
    }

    // Snel inloggen op een lokaal opgeslagen account.
    // Let op: deze functie slaat de wachtwoordcontrole over.
    public bool QuickLogin(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        foreach (PlayerAccount account in database.accounts)
        {
            if (account.username == username)
            {
                currentAccount = account;
                return true;
            }
        }

        return false;
    }

    public void SaveCurrentAccount()
    {
        if (currentAccount == null)
            return;

        SaveAccounts();
    }

    public void Logout()
    {
        SaveCurrentAccount();
        currentAccount = null;
    }
}
