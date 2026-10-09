
using Unity.Netcode;
using UnityEngine;

public class YeetBase : NetworkBehaviour
{
    [Header("Yeet Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float time = 5f;
    [SerializeField] private float damage = 10f;

    private float timer;

    private void Update()
    {
        // Alleen de server bestuurt de projectile.
        if (!IsServer)
            return;

        transform.position +=
            transform.forward * speed * Time.deltaTime;

        timer += Time.deltaTime;

        if (timer >= time)
        {
            RemoveProjectile();
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
            return;

        PlayerHealth playerHealth =
            collision.gameObject.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            Debug.Log($"Speler geraakt! Damage: {damage}");

            playerHealth.Damage(damage, OwnerClientId);
        }

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void RemoveProjectile()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}