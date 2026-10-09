
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Image Healthbar;

    public NetworkVariable<float> Health = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public float health => isNetworked ? Health.Value : localHealth;

    [Header("Multiplayer")]
    [SerializeField] private TestLObby testLobby;
    [SerializeField] private MPRoundSystem mpRoundSystem;

    private MultiplayerGame multiplayerGame;

    private bool isNetworked;
    private bool deathRegistered;
    private float localHealth;

    private void Awake()
    {
        isNetworked =
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening;

        if (!isNetworked)
        {
            localHealth = maxHealth;
            UpdateUI(localHealth);
        }
    }

    public override void OnNetworkSpawn()
    {
        isNetworked = true;

        if (testLobby == null)
            testLobby = FindAnyObjectByType<TestLObby>();

        if (mpRoundSystem == null)
            mpRoundSystem = FindAnyObjectByType<MPRoundSystem>();

        multiplayerGame = FindAnyObjectByType<MultiplayerGame>();

        if (IsServer)
        {
            Health.Value = maxHealth;
            deathRegistered = false;
        }

        Health.OnValueChanged += OnHealthChanged;

        if (IsOwner && testLobby != null)
            SendPlayerIdServerRpc(testLobby.playerName);

        UpdateUI(Health.Value);
    }

    public override void OnNetworkDespawn()
    {
        Health.OnValueChanged -= OnHealthChanged;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        if (!isNetworked)
        {
            localHealth = maxHealth;
            deathRegistered = false;
            UpdateUI(localHealth);
            return;
        }

        if (IsServer)
        {
            Health.Value = maxHealth;
            deathRegistered = false;
        }
    }

    public void DisableUI()
    {
        if (Healthbar != null && Healthbar.transform.parent != null)
            Healthbar.transform.parent.gameObject.SetActive(false);
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        if (IsOwner)
            UpdateUI(newValue);
    }

    private void UpdateUI(float value)
    {
        if (Healthbar == null)
            return;

        float hp = Mathf.Clamp(value, 0f, maxHealth);

        Healthbar.fillAmount = maxHealth > 0f ? hp / maxHealth : 0f;
        Healthbar.color = hp <= 40f ? Color.red : Color.green;
    }

    // Damage zonder aanvaller, bijvoorbeeld omgevingsdamage.
    public void Damage(float amount)
    {
        Damage(amount, ulong.MaxValue);
    }

    // Damage met de ClientId van de aanvaller.
    public void Damage(float amount, ulong attackerClientId)
    {
        if (amount <= 0f)
            return;

        if (!isNetworked)
        {
            ApplyLocalDamage(amount);
            return;
        }

        if (IsServer)
        {
            ApplyDamage(amount, attackerClientId);
        }
        else
        {
            // De server gebruikt de afzender van de RPC
            // niet automatisch als aanvaller van deze damage.
            TakeDamageServerRpc(amount);
        }
    }

    private void ApplyLocalDamage(float amount)
    {
        if (localHealth <= 0f)
            return;

        localHealth = Mathf.Max(0f, localHealth - amount);
        UpdateUI(localHealth);

        if (localHealth <= 0f)
            Debug.Log("Speler is dood.");
    }

    private void ApplyDamage(float amount, ulong attackerClientId)
    {
        if (!IsServer || Health.Value <= 0f)
            return;

        Health.Value = Mathf.Max(0f, Health.Value - amount);

        Debug.Log(
            $"Speler {OwnerClientId}: {Health.Value}/{maxHealth} HP"
        );

        if (Health.Value <= 0f && !deathRegistered)
        {
            deathRegistered = true;
            RegisterDeath(attackerClientId);
        }
    }

    private void RegisterDeath(ulong attackerClientId)
    {
        if (!IsServer)
            return;

        Debug.Log($"Speler {OwnerClientId} is dood.");

        if (multiplayerGame == null)
            multiplayerGame = FindAnyObjectByType<MultiplayerGame>();

        if (multiplayerGame == null)
            return;

        multiplayerGame.AddDeath(OwnerClientId);

        if (attackerClientId != ulong.MaxValue &&
            attackerClientId != OwnerClientId)
        {
            multiplayerGame.AddKill(attackerClientId);
        }
    }

    [ServerRpc]
    private void TakeDamageServerRpc(float amount)
    {
        // Client aangevraagde damage heeft hier geen
        // geverifieerde aanvaller-ID.
        ApplyDamage(amount, ulong.MaxValue);
    }

    [ServerRpc]
    private void SendPlayerIdServerRpc(
        string playerName,
        ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (mpRoundSystem != null)
            mpRoundSystem.RegisterPlayerName(clientId, playerName);
    }
}