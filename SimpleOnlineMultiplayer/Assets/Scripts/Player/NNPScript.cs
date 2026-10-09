
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NNPScript : NetworkBehaviour
{
    [Header("Movement")]
    public float Speed = 10f;

    [SerializeField] private Transform cameraPos;

    private Vector3 Movement;
    private Rigidbody rb;
    private PlayerInput Controls;
    private bool localMode;

    [Header("Inventory")]
    [SerializeField] private int inventorySize = 3;

    private readonly List<GameObject> inventory = new List<GameObject>();
    private int selectedInventorySlot = 0;

    [Header("Player UI")]
    [SerializeField] private PlayerUI playerUI;

    [Header("Shoot")]
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private GameObject yeetPrefab;

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        Controls = new PlayerInput();

        Controls.Player.Walk.performed += OnMove;
        Controls.Player.Walk.canceled += OnMove;
        Controls.Player.Inventory.performed += OnInventory;
        Controls.Player.Shoot.performed += OnShoot;

        localMode =
            NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening;

        if (localMode)
            Controls.Player.Enable();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Controls.Player.Enable();
        }
        else
        {
            Controls.Player.Disable();
        }
    }

    public override void OnNetworkDespawn()
    {
        Controls.Player.Disable();
    }

    private void OnDestroy()
    {
        if (Controls == null)
            return;

        Controls.Player.Walk.performed -= OnMove;
        Controls.Player.Walk.canceled -= OnMove;
        Controls.Player.Inventory.performed -= OnInventory;
        Controls.Player.Shoot.performed -= OnShoot;

        Controls.Player.Disable();
        Controls.Dispose();
    }

    // =========================
    // MOVEMENT
    // =========================

    public void OnMove(InputAction.CallbackContext context)
    {
        if (IsSpawned && !IsOwner)
            return;

        Vector2 input = context.ReadValue<Vector2>();

        Movement.x = input.x;
        Movement.z = input.y;
    }

    private void FixedUpdate()
    {
        if (IsSpawned && !IsOwner)
            return;

        if (rb == null || cameraPos == null)
            return;

        Vector3 forward = cameraPos.forward;
        Vector3 right = cameraPos.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 movement =
            right * Movement.x + forward * Movement.z;

        rb.MovePosition(
            rb.position + movement * Speed * Time.fixedDeltaTime
        );
    }

    // =========================
    // INVENTORY INPUT
    // =========================

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (IsSpawned && !IsOwner)
            return;

        if (!context.performed)
            return;

        string key = context.control.name;

        if (key == "1")
            SelectInventorySlot(0);
        else if (key == "2")
            SelectInventorySlot(1);
        else if (key == "3")
            SelectInventorySlot(2);
    }

    private void SelectInventorySlot(int slot)
    {
        if (slot < 0 || slot >= inventory.Count)
        {
            Debug.Log("Dit inventory-slot is leeg!");
            return;
        }

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
        if (IsSpawned && !IsOwner)
            return;

        if (!context.performed)
            return;

        Shoot();
    }

    private void Shoot()
    {
        if (inventory.Count == 0)
        {
            Debug.Log("Inventory is leeg!");
            return;
        }

        if (selectedInventorySlot < 0 ||
            selectedInventorySlot >= inventory.Count)
        {
            Debug.Log("Dit inventory-slot is leeg!");
            return;
        }

        if (shootingPoint == null)
        {
            Debug.LogWarning("Shooting Point is niet ingesteld!");
            return;
        }

        if (yeetPrefab == null)
        {
            Debug.LogWarning("Yeet Prefab is niet ingesteld!");
            return;
        }

        GameObject itemToShoot = inventory[selectedInventorySlot];

        if (itemToShoot == null)
        {
            Debug.LogWarning("Item in inventory is null!");
            return;
        }

        if (IsSpawned)
        {
            ShootServerRpc(
                shootingPoint.position,
                shootingPoint.rotation
            );
        }
        else
        {
            Instantiate(
                yeetPrefab,
                shootingPoint.position,
                shootingPoint.rotation
            );
        }

        Debug.Log($"Ik schiet een item vanuit slot {selectedInventorySlot + 1}");

        inventory.RemoveAt(selectedInventorySlot);

        if (inventory.Count == 0)
            selectedInventorySlot = 0;
        else if (selectedInventorySlot >= inventory.Count)
            selectedInventorySlot = inventory.Count - 1;

        UpdateInventoryUI();
    }

    [ServerRpc]
    private void ShootServerRpc(
        Vector3 position,
        Quaternion rotation)
    {
        if (yeetPrefab == null)
            return;

        GameObject projectile = Instantiate(
            yeetPrefab,
            position,
            rotation
        );

        NetworkObject networkObject =
            projectile.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError(
                "Yeet Prefab heeft geen NetworkObject!"
            );

            Destroy(projectile);
            return;
        }

        // De eigenaar blijft herkenbaar als aanvaller.
        networkObject.SpawnWithOwnership(OwnerClientId);
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

        if (inventory.Count >= inventorySize)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        inventory.Add(itemPrefab);

        Debug.Log(
            $"[{gameObject.name}] {itemPrefab.name} opgepakt! " +
            $"Inventory: {inventory.Count}/{inventorySize}"
        );

        UpdateInventoryUI();
        return true;
    }

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
                amount++;
        }

        return amount;
    }

    public bool RemoveItem(GameObject itemPrefab)
    {
        if (!inventory.Contains(itemPrefab))
            return false;

        inventory.Remove(itemPrefab);

        Debug.Log($"{itemPrefab.name} verwijderd uit inventory.");

        if (selectedInventorySlot >= inventory.Count)
            selectedInventorySlot = Mathf.Max(0, inventory.Count - 1);

        UpdateInventoryUI();
        return true;
    }

    private void UpdateInventoryUI()
    {
        if (playerUI != null)
            playerUI.UpdateInventoryUI(inventory);
    }

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
            Debug.Log($"Slot {i + 1}: {inventory[i].name}");
        }

        Debug.Log($"Geselecteerd slot: {selectedInventorySlot + 1}");
    }
}