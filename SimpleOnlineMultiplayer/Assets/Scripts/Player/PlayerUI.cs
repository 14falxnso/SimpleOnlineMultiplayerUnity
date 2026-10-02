using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [System.Serializable]
    public class ItemVisual
    {
        public GameObject itemPrefab;
        public Sprite itemSprite;
    }

    [Header("Vak 1")]
    [SerializeField] private Image showItemVak1;

    [Header("Vak 2")]
    [SerializeField] private Image showItemVak2;

    [Header("Vak 3")]
    [SerializeField] private Image showItemVak3;

    [Header("Items")]
    [SerializeField] private List<ItemVisual> itemVisuals;

    public void UpdateInventoryUI(List<GameObject> inventory)
    {
        // Eerst alle vakken leegmaken
        showItemVak1.enabled = false;
        showItemVak2.enabled = false;
        showItemVak3.enabled = false;

        // Vak 1
        if (inventory.Count > 0)
        {
            ShowItem(showItemVak1, inventory[0]);
        }

        // Vak 2
        if (inventory.Count > 1)
        {
            ShowItem(showItemVak2, inventory[1]);
        }

        // Vak 3
        if (inventory.Count > 2)
        {
            ShowItem(showItemVak3, inventory[2]);
        }
    }

    private void ShowItem(Image image, GameObject item)
    {
        Sprite sprite = GetItemSprite(item);

        if (sprite == null)
            return;

        image.sprite = sprite;
        image.enabled = true;
    }

    private Sprite GetItemSprite(GameObject item)
    {
        foreach (ItemVisual visual in itemVisuals)
        {
            if (visual.itemPrefab == item)
            {
                return visual.itemSprite;
            }
        }

        Debug.LogWarning(
            "Geen Sprite gekoppeld aan: " + item.name
        );

        return null;
    }
}
