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

        // Sla het bestand op in de hoofdmap van je Unity-project.
        string projectFolder = Directory.GetParent(Application.dataPath).FullName;
        string saveFolder = Path.Combine(projectFolder, "SavedData");

        Directory.CreateDirectory(saveFolder);

        filePath = Path.Combine(saveFolder, "accounts.json");

        // Neem eventueel je bestaande JSON-bestand mee.
        string oldFilePath = Path.Combine(
            Application.persistentDataPath,
            "accounts.json"
        );

        if (!File.Exists(filePath) && File.Exists(oldFilePath))
        {
            File.Copy(oldFilePath, filePath);
            Debug.Log("Bestaande accounts.json gekopieerd.");
        }

        Debug.Log("Accounts JSON locatie: " + filePath);

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
            database.accounts = new System.Collections.Generic.List<PlayerAccount>();

        Debug.Log("Accounts geladen.");
    }

    private void SaveAccounts()
    {
        string json = JsonUtility.ToJson(database, true);
        File.WriteAllText(filePath, json);

        Debug.Log("Accounts opgeslagen: " + filePath);
    }

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
