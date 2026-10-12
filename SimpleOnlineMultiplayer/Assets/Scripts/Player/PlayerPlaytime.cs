using Unity.Netcode;
using UnityEngine;

public class PlayerPlaytime : NetworkBehaviour
{
    private float saveTimer;
    private const float SaveInterval = 30f;

    private void Update()
    {
        if (!IsOwner)
            return;

        if (AccountManager.Instance == null ||
            AccountManager.Instance.CurrentAccount == null)
            return;

        PlayerAccount account =
            AccountManager.Instance.CurrentAccount;

        if (account.stats == null)
            return;

        account.stats.playtime += Time.deltaTime;
        saveTimer += Time.deltaTime;

        if (saveTimer >= SaveInterval)
        {
            AccountManager.Instance.SaveCurrentAccount();
            saveTimer = 0f;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner &&
            AccountManager.Instance != null &&
            AccountManager.Instance.CurrentAccount != null)
        {
            AccountManager.Instance.SaveCurrentAccount();
        }

        base.OnNetworkDespawn();
    }
}
