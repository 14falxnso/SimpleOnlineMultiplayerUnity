using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MapSpawner : NetworkBehaviour
{
    [SerializeField] private List<GameObject> mapList;
    private GameObject selectedMap;

    [SerializeField] private TMP_Dropdown dropdown;
    public string mapChoice;

    public void RandomMapSelector()
    {
        selectedMap = mapList[Random.Range(0, mapList.Count)];
    }

    public void SelectMap()
    {
        mapChoice = dropdown.options[dropdown.value].text;
        if (mapChoice == "City")
        {
            selectedMap = mapList[0];
        }

        if (mapChoice == "River")
        {
            selectedMap = mapList[1];
        }

        if (mapChoice == "SoccerField")
        {
            selectedMap = mapList[2];
        }

        if (IsServer)
        {
            Vector3 spawnMapPos = Vector3.zero;

            Instantiate(selectedMap, spawnMapPos, Quaternion.identity);
        }
    }
}
