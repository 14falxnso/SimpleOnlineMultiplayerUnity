using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NNPScript : NetworkBehaviour
{
    [Header("Movement")]
    public float Speed = 10f;

    private Vector3 Movement;
    private Rigidbody rb;

    private PlayerInput Controls;

    [SerializeField] private Transform cameraPos;


    [Header("Inventory")]
    [SerializeField] private int inventorySize = 3;

    private List<GameObject> inventory = new List<GameObject>();

    // Welk inventory-slot momenteel geselecteerd is
    // 0 = vak 1
    // 1 = vak 2
    // 2 = vak 3
    private int selectedInventorySlot = 0;


    [Header("Player UI")]
    [SerializeField] private PlayerUI playerUI;


    [Header("Shoot")]
    [SerializeField] private Transform shootingPoint;

    // Dit is de prefab die daadwerkelijk wordt geschoten
    [SerializeField] private GameObject yeetPrefab;

    [SerializeField] private float yeetSpeed = 10f;


    private void Awake()
    {
        // Rigidbody zoeken, ook als deze op een child zit
        rb = GetComponentInChildren<Rigidbody>();

        // Input System aanmaken
        Controls = new PlayerInput();

        // Movement
        Controls.Player.Walk.performed += OnMove;
        Controls.Player.Walk.canceled += OnMove;

        // Inventory
        Controls.Player.Inventory.performed += OnInventory;


        // Shoot
        Controls.Player.Shoot.performed += OnShoot;

        // Input aanzetten
        Controls.Player.Enable();
    }


    private void OnDestroy()
    {
        if (Controls != null)
        {
            // Movement
            Controls.Player.Walk.performed -= OnMove;
            Controls.Player.Walk.canceled -= OnMove;

            // Inventory
            Controls.Player.Inventory.performed -= OnInventory;


            // Shoot
            Controls.Player.Shoot.performed -= OnShoot;

            Controls.Player.Disable();
        }
    }


    // =========================
    // MOVEMENT
    // =========================

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        Movement.x = input.x;
        Movement.z = input.y;
    }


    private void FixedUpdate()
    {
        if (rb == null || cameraPos == null)
            return;

        // Richtingen van de camera pakken
        Vector3 forward = cameraPos.forward;
        Vector3 right = cameraPos.right;

        // Zorgen dat we alleen over de grond bewegen
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Input omzetten naar camera-relative movement
        Vector3 movement = (right * Movement.x) + (forward * Movement.z);

        // Rigidbody bewegen
        rb.MovePosition(
            rb.position + movement * Speed * Time.fixedDeltaTime
        );
    }


    // =========================
    // INVENTORY INPUT
    // =========================

    // Toets 1
    // =========================
    // INVENTORY INPUT
    // =========================

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // Kijken welke binding is gebruikt
        string key = context.control.name;

        if (key == "1")
        {
            SelectInventorySlot(0);
        }
        else if (key == "2")
        {
            SelectInventorySlot(1);
        }
        else if (key == "3")
        {
            SelectInventorySlot(2);
        }
    }


    // Toets 2
    public void OnInventory2(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        SelectInventorySlot(1);
    }


    // Toets 3
    public void OnInventory3(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        SelectInventorySlot(2);
    }


    // =========================
    // INVENTORY SELECTEREN
    // =========================

    private void SelectInventorySlot(int slot)
    {
        // Check of het slot bestaat
        if (slot < 0 || slot >= inventory.Count)
        {
            Debug.Log("Dit inventory-slot is leeg!");
            return;
        }

        // Slot opslaan
        selectedInventorySlot = slot;

        Debug.Log(
            $"Inventory slot {slot + 1} geselecteerd: " +
            inventory[selectedInventorySlot].name
        );
    }


    // =========================
    // SHOOT INPUT
    // =========================

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Shoot();
    }


    // =========================
    // SHOOT
    // =========================

    private void Shoot()
    {
        // Check of inventory leeg is
        if (inventory.Count == 0)
        {
            Debug.Log("Inventory is leeg!");
            return;
        }

        // Check of geselecteerde slot bestaat
        if (selectedInventorySlot >= inventory.Count)
        {
            Debug.Log("Dit inventory-slot is leeg!");
            return;
        }

        // Pak het geselecteerde item
        GameObject itemToShoot = inventory[selectedInventorySlot];

        if (itemToShoot == null)
        {
            Debug.LogWarning("Item in inventory is null!");
            return;
        }

        if (shootingPoint == null)
        {
            Debug.LogWarning("Shooting Point is niet ingesteld!");
            return;
        }

        Debug.Log(
            $"Ik schiet {itemToShoot.name} vanuit slot {selectedInventorySlot + 1}"
        );

        // Item spawnen
        Instantiate(
            itemToShoot,
            shootingPoint.position,
            shootingPoint.rotation
        );

        // Item uit inventory verwijderen
        inventory.RemoveAt(selectedInventorySlot);

        // Nieuw geselecteerd slot bepalen
        if (inventory.Count == 0)
        {
            selectedInventorySlot = 0;
        }
        else if (selectedInventorySlot >= inventory.Count)
        {
            selectedInventorySlot = inventory.Count - 1;
        }

        // UI updaten
        if (playerUI != null)
        {
            playerUI.UpdateInventoryUI(inventory);
        }
    }


    // =========================
    // INVENTORY
    // =========================

    public bool AddItem(GameObject itemPrefab)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning("Item prefab is null!");
            return false;
        }

        // Check of inventory vol zit
        if (inventory.Count >= inventorySize)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        // Item toevoegen
        inventory.Add(itemPrefab);

        Debug.Log(
            $"[{gameObject.name}] {itemPrefab.name} opgepakt! " +
            $"Inventory: {inventory.Count}/{inventorySize}"
        );

        // UI updaten
        if (playerUI != null)
        {
            playerUI.UpdateInventoryUI(inventory);
        }
        else
        {
            Debug.LogWarning(
                "PlayerUI is niet gekoppeld aan " +
                gameObject.name
            );
        }

        return true;
    }


    // =========================
    // INVENTORY CHECKS
    // =========================

    public bool HasItem(GameObject itemPrefab)
    {
        return inventory.Contains(itemPrefab);
    }


    public int GetItemAmount(GameObject itemPrefab)
    {
        int amount = 0;

        foreach (GameObject item in inventory)
        {
            if (item == itemPrefab)
            {
                amount++;
            }
        }

        return amount;
    }


    public bool RemoveItem(GameObject itemPrefab)
    {
        if (!inventory.Contains(itemPrefab))
            return false;

        inventory.Remove(itemPrefab);

        Debug.Log(
            $"{itemPrefab.name} verwijderd uit inventory."
        );

        // UI opnieuw updaten
        if (playerUI != null)
        {
            playerUI.UpdateInventoryUI(inventory);
        }

        return true;
    }


    // =========================
    // INVENTORY DEBUG
    // =========================

    [ContextMenu("Show Inventory")]
    private void ShowInventory()
    {
        Debug.Log("===== INVENTORY =====");

        if (inventory.Count == 0)
        {
            Debug.Log("Inventory is leeg.");
            return;
        }

        for (int i = 0; i < inventory.Count; i++)
        {
            Debug.Log(
                $"Slot {i + 1}: {inventory[i].name}"
            );
        }

        Debug.Log(
            $"Geselecteerd slot: {selectedInventorySlot + 1}"
        );
    }
}