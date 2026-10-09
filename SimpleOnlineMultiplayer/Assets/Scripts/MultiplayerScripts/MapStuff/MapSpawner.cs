
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class MapSpawner : NetworkBehaviour
{
    [Header("Maps")]
    [SerializeField] private List<GameObject> mapList;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown dropdown;

    public string mapChoice;

    private NetworkObject spawnedMap;

    public void RandomMapSelector()
    {
        if (!IsServer)
            return;

        if (mapList == null || mapList.Count == 0)
        {
            Debug.LogWarning("Er zijn geen maps ingesteld!");
            return;
        }

        int randomIndex = Random.Range(0, mapList.Count);
        SpawnMap(mapList[randomIndex]);
    }

    public void SelectMap()
    {
        // Deze functie kan door een UI-button worden aangeroepen.
        // De server bepaalt uiteindelijk welke map gespawned wordt.
        if (dropdown == null)
        {
            Debug.LogWarning("Map dropdown is niet ingesteld!");
            return;
        }

        mapChoice = dropdown.options[dropdown.value].text;

        if (IsServer)
        {
            SelectMapOnServer(mapChoice);
        }
        else
        {
            SelectMapServerRpc(mapChoice);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectMapServerRpc(string choice)
    {
        SelectMapOnServer(choice);
    }

    private void SelectMapOnServer(string choice)
    {
        if (!IsServer)
            return;

        int index = -1;

        switch (choice)
        {
            case "City":
                index = 0;
                break;

            case "River":
                index = 1;
                break;

            case "SoccerField":
                index = 2;
                break;
        }

        if (index < 0 || index >= mapList.Count)
        {
            Debug.LogWarning($"Onbekende map: {choice}");
            return;
        }

        mapChoice = choice;
        SpawnMap(mapList[index]);
    }

    private void SpawnMap(GameObject mapPrefab)
    {
        if (!IsServer || mapPrefab == null)
            return;

        // Oude map opruimen.
        if (spawnedMap != null && spawnedMap.IsSpawned)
        {
            spawnedMap.Despawn(true);
        }

        GameObject mapInstance = Instantiate(
            mapPrefab,
            Vector3.zero,
            Quaternion.identity
        );

        NetworkObject networkObject =
            mapInstance.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError(
                $"Map prefab '{mapPrefab.name}' heeft geen NetworkObject."
            );

            Destroy(mapInstance);
            return;
        }

        networkObject.Spawn();
        spawnedMap = networkObject;

        Debug.Log($"Map '{mapPrefab.name}' is gespawned.");
    }
}