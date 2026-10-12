using Unity.Netcode;
using UnityEngine;

public class NetworkPlaytimeTracker : MonoBehaviour
{
    [SerializeField] private float saveInterval = 30f;

    private NetworkManager networkManager;

    private bool isTracking;
    private float unsavedSeconds;
    private float saveTimer;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;

        if (networkManager == null)
            return;

        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;

        // Voor het geval de speler al verbonden is.
        if (networkManager.IsClient &&
            networkManager.IsConnectedClient)
        {
            StartTracking();
        }
    }

    private void Update()
    {
        if (!isTracking)
            return;

        unsavedSeconds += Time.unscaledDeltaTime;
        saveTimer += Time.unscaledDeltaTime;

        if (saveTimer >= saveInterval)
        {
            SaveElapsedTime();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (networkManager == null)
            return;

        // Alleen de lokale speler bijhouden.
        if (clientId == networkManager.LocalClientId)
        {
            StartTracking();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (networkManager == null)
            return;

        if (clientId == networkManager.LocalClientId)
        {
            StopTracking();
        }
    }

    private void StartTracking()
    {
        if (isTracking)
            return;

        if (AccountManager.Instance == null ||
            AccountManager.Instance.CurrentAccount == null)
            return;

        isTracking = true;
        unsavedSeconds = 0f;
        saveTimer = 0f;
    }

    private void StopTracking()
    {
        if (!isTracking)
            return;

        SaveElapsedTime();
        isTracking = false;
    }

    private void SaveElapsedTime()
    {
        if (unsavedSeconds <= 0f)
            return;

        if (AccountManager.Instance == null ||
            AccountManager.Instance.CurrentAccount == null)
        {
            unsavedSeconds = 0f;
            saveTimer = 0f;
            return;
        }

        PlayerAccount account =
            AccountManager.Instance.CurrentAccount;

        if (account.stats == null)
        {
            unsavedSeconds = 0f;
            saveTimer = 0f;
            return;
        }

        account.stats.playtime += unsavedSeconds;

        unsavedSeconds = 0f;
        saveTimer = 0f;

        AccountManager.Instance.SaveCurrentAccount();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && isTracking)
        {
            SaveElapsedTime();
        }
    }

    private void OnApplicationQuit()
    {
        if (isTracking)
        {
            SaveElapsedTime();
        }
    }

    private void OnDestroy()
    {
        if (isTracking)
        {
            SaveElapsedTime();
        }

        if (networkManager != null)
        {
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
