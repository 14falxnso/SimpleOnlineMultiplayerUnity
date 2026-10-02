using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> pickupObjects;

    public float Radius = 1;

    public float SpawnTimer;
    public float requiredTime = 4f;



    void Update()
    {
        SpawnTimer += Time.deltaTime;

        if (SpawnTimer >= requiredTime) 
        { 
            SpawnRandomPickup();
            SpawnTimer = 0;
        }
    }

    private void SpawnRandomPickup()
    {
        var offset2D = UnityEngine.Random.insideUnitCircle * Radius;
        var spawnPos = new Vector3(offset2D.x, 0f, offset2D.y) + transform.position;
        Instantiate(pickupObjects[UnityEngine.Random.Range(0, pickupObjects.Count)], spawnPos, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }

}
