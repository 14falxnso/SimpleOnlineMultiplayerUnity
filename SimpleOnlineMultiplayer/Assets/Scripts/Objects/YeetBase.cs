using UnityEngine;
using Unity.Netcode;

public class YeetBase : NetworkBehaviour
{
    [Header("Yeet Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float time = 5f;

    private float timer;

    private void Update()
    {
        // Object vooruit bewegen
        transform.position += transform.forward * speed * Time.deltaTime;

        // Timer
        timer += Time.deltaTime;

        // Na bepaalde tijd verwijderen
        if (timer >= time)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        NNPScript player = collision.gameObject.GetComponentInParent<NNPScript>();

        if (player == null)
            return;

        Debug.Log($"{gameObject.name} raakt speler!");

        // Later:
        // player.TakeDamage(damage);

        Destroy(gameObject);
    }
}