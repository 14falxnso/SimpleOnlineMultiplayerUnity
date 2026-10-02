using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSpawner : MonoBehaviour
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
    }
}
