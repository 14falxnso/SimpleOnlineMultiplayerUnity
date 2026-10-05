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

        filePath = Path.Combine(Application.persistentDataPath, "accounts.json");

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
        {
            database = new AccountDatabase();
        }
    }

    private void SaveAccounts()
    {
        string json = JsonUtility.ToJson(database, true);

        File.WriteAllText(filePath, json);
    }

    public bool Register(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        if (string.IsNullOrWhiteSpace(password))
            return false;

        foreach (PlayerAccount account in database.accounts)
        {
            if (account.username == username)
            {
                return false;
            }
        }

        PlayerAccount newAccount = new PlayerAccount(username, password);

        database.accounts.Add(newAccount);

        SaveAccounts();

        currentAccount = newAccount;

        return true;
    }

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
