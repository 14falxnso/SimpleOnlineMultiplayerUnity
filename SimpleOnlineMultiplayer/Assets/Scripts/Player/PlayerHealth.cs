using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    public NetworkVariable<float> Health =
        new NetworkVariable<float>();

    [SerializeField] private Image Healthbar;
    [SerializeField] private ParticleSystem PlayerHitEffect;

    public DamageTypes lastDamageType =
        DamageTypes.General;

    [Header("Multiplayer Round System")]
    [SerializeField] private TestLObby testLobby;
    [SerializeField] private MPRoundSystem mpRoundSystem;

    private bool isNetworked;

    private float localHealth;

    public float health =>
        isNetworked
            ? Health.Value
            : localHealth;

    private void Awake()
    {
        isNetworked =
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening;

        if (!isNetworked)
        {
            localHealth = maxHealth;
            UpdateUI();
        }
    }

    public override void OnNetworkSpawn()
    {
        isNetworked = true;

        if (testLobby == null)
            testLobby =
                FindAnyObjectByType<TestLObby>();

        if (mpRoundSystem == null)
            mpRoundSystem =
                FindAnyObjectByType<MPRoundSystem>();

        if (IsServer)
        {
            Health.Value = maxHealth;
        }

        Health.OnValueChanged += OnHealthChanged;

        if (IsOwner)
        {
            string myName =
                testLobby != null
                    ? testLobby.playerName
                    : "Speler";

            SendPlayerIdServerRpc(myName);
        }

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

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        if (!isNetworked)
        {
            localHealth = maxHealth;
            UpdateUI();
            return;
        }

        if (IsServer)
        {
            Health.Value = maxHealth;
            lastDamageType = DamageTypes.General;
        }
    }

    public void DisableUI()
    {
        if (Healthbar != null &&
            Healthbar.transform.parent != null)
        {
            Healthbar.transform.parent.gameObject.SetActive(false);
        }
    }

    private void OnHealthChanged(
        float oldValue,
        float newValue)
    {
        if (!IsOwner)
            return;

        UpdateUI(newValue);
    }

    private void UpdateUI(
        float value = -1f)
    {
        if (Healthbar == null)
            return;

        float hp =
            value < 0f
                ? localHealth
                : value;

        Healthbar.fillAmount =
            Mathf.Clamp01(hp / maxHealth);

        Healthbar.color =
            hp <= 40f
                ? Color.red
                : Color.green;
    }

    public void Damage(
        float amount,
        DamageTypes type = DamageTypes.General)
    {
        if (amount <= 0f)
            return;

        if (!isNetworked)
        {
            ApplyLocalDamage(amount, type);
            return;
        }

        if (IsServer)
        {
            ApplyDamage(amount, type);
        }
        else
        {
            TakeDamageServerRpc(amount, type);
        }
    }

    private void ApplyLocalDamage(
        float amount,
        DamageTypes type)
    {
        localHealth -= amount;
        localHealth = Mathf.Max(0f, localHealth);

        lastDamageType = type;

        UpdateUI();
    }

    private void ApplyDamage(
        float amount,
        DamageTypes type)
    {
        if (!IsServer)
            return;

        Health.Value -= amount;
        Health.Value = Mathf.Max(0f, Health.Value);

        lastDamageType = type;

        if (PlayerHitEffect != null)
        {
            PlayerHitEffect.Play();
        }
    }

    [ServerRpc]
    private void TakeDamageServerRpc(
        float amount,
        DamageTypes type)
    {
        ApplyDamage(amount, type);
    }

    [ServerRpc]
    private void SendPlayerIdServerRpc(
        string playerName,
        ServerRpcParams rpcParams = default)
    {
        ulong clientId =
            rpcParams.Receive.SenderClientId;

        if (mpRoundSystem != null)
        {
            mpRoundSystem.RegisterPlayerName(
                clientId,
                playerName
            );
        }
    }
}

public enum DamageTypes
{
    Melee,
    Ranged,
    Fire,
    Freeze,
    Fall,
    General
}