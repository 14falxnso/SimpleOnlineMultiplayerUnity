using Unity.Netcode;
using UnityEngine;

public class BasePickup : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float speed = 100f;

    private void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PICKUP GERAAKT DOOR: " + other.gameObject.name);

        NNPScript player = other.GetComponentInParent<NNPScript>();

        if (player == null)
            return;

        Debug.Log("PLAYER GEVONDEN!");

        if (player.AddItem(prefab))
        {
            Debug.Log("ITEM OPGEPAKT!");

            Destroy(gameObject);
        }
    }
}