using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private InputActionReference walkAction;
    [SerializeField] private InputActionReference jumpAction;

    [SerializeField] private float moveSpeed = 3f;

    [Header("Outfits")]
    [SerializeField] private GameObject[] outfitPrefabs;

    private GameObject currentOutfit;

    [Header("Player Name In Game")]
    [SerializeField] private TextMeshProUGUI playerNameText;

    // Dit is de naam die iedereen van deze player kan zien.
    private NetworkVariable<FixedString64Bytes> playerName =
        new NetworkVariable<FixedString64Bytes>(
            "",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private NetworkVariable<MyCustomData> randomNumber =
        new NetworkVariable<MyCustomData>(
            new MyCustomData
            {
                _int = 0,
                _bool = true
            },
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;

        public void NetworkSerialize<T>(
            BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += OnRandomNumberChanged;
        playerName.OnValueChanged += OnPlayerNameChanged;

        // Naam meteen tonen als deze al beschikbaar is.
        UpdatePlayerNameUI(playerName.Value.ToString());

        // Alleen de eigenaar gebruikt zijn eigen input.
        if (IsOwner)
        {
            if (walkAction != null)
                walkAction.action.Enable();

            if (jumpAction != null)
                jumpAction.action.Enable();

            // Vraag aan de server om onze naam in te stellen.
            SendPlayerNameToServer();
        }

        Debug.Log(
            $"Player spawned | " +
            $"ClientId: {OwnerClientId} | " +
            $"IsOwner: {IsOwner} | " +
            $"IsServer: {IsServer}"
        );
    }

    public override void OnNetworkDespawn()
    {
        randomNumber.OnValueChanged -= OnRandomNumberChanged;
        playerName.OnValueChanged -= OnPlayerNameChanged;

        if (IsOwner)
        {
            if (walkAction != null)
                walkAction.action.Disable();

            if (jumpAction != null)
                jumpAction.action.Disable();
        }
    }

    // =========================================================
    // PLAYER NAME
    // =========================================================

    private void SendPlayerNameToServer()
    {
        TestLObby lobby = FindFirstObjectByType<TestLObby>();

        if (lobby == null)
        {
            Debug.LogWarning("TestLObby not found.");
            return;
        }

        lobby.SendPlayerNameToServerRpc(
            lobby.playerName
        );
    }

    private void OnPlayerNameChanged(
        FixedString64Bytes previousValue,
        FixedString64Bytes newValue)
    {
        UpdatePlayerNameUI(newValue.ToString());
    }

    private void UpdatePlayerNameUI(string newName)
    {
        if (playerNameText == null)
            return;

        if (string.IsNullOrWhiteSpace(newName))
        {
            playerNameText.text = "Player";
        }
        else
        {
            playerNameText.text = newName;
        }
    }

    public string GetPlayerName()
    {
        return playerName.Value.ToString();
    }

    public void SetNetworkName(string newName)
    {
        if (!IsServer)
            return;

        if (string.IsNullOrWhiteSpace(newName))
            newName = "Player";

        playerName.Value = newName;
    }

    // =========================================================
    // RANDOM NUMBER
    // =========================================================

    private void OnRandomNumberChanged(
        MyCustomData previousValue,
        MyCustomData newValue)
    {
        Debug.Log(
            $"Player {OwnerClientId} data changed: " +
            $"{newValue._int}, {newValue._bool}"
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!IsSpawned)
            return;

        if (!IsOwner)
            return;

        HandleMovement();
        HandleJump();

        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            TestServerRpc();
        }
    }

    private void HandleMovement()
    {
        if (walkAction == null)
            return;

        Vector2 input =
            walkAction.action.ReadValue<Vector2>();

        Vector3 moveDir =
            new Vector3(input.x, 0f, input.y);

        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        transform.position +=
            moveDir *
            moveSpeed *
            Time.deltaTime;
    }

    private void HandleJump()
    {
        if (jumpAction == null)
            return;

        if (jumpAction.action.WasPressedThisFrame())
        {
            Debug.Log(
                $"Jump pressed by client {OwnerClientId}"
            );
        }
    }

    // =========================================================
    // SERVER RPC
    // =========================================================

    [ServerRpc]
    private void TestServerRpc()
    {
        Debug.Log(
            "T pressed by client: " +
            OwnerClientId
        );
    }

    // =========================================================
    // OUTFIT
    // =========================================================

    [ServerRpc]
    public void ChangeOutfitServerRpc(string outfitName)
    {
        //GameObject prefab = null;

        //foreach (GameObject outfit in outfitPrefabs)
        //{
        //    if (outfit != null &&
        //        outfit.name == outfitName)
        //    {
        //        prefab = outfit;
        //        break;
        //    }
        //}

        //if (prefab == null)
        //{
        //    Debug.LogWarning(
        //        "Outfit not found: " +
        //        outfitName
        //    );

        //    return;
        //}

        //if (currentOutfit != null)
        //{
        //    NetworkObject oldNetworkObject =
        //        currentOutfit.GetComponent<NetworkObject>();

        //    if (oldNetworkObject != null &&
        //        oldNetworkObject.IsSpawned)
        //    {
        //        oldNetworkObject.Despawn(true);
        //    }

        //    Destroy(currentOutfit);
        //    currentOutfit = null;
        //}

        //GameObject newOutfit =
        //    Instantiate(prefab, transform);

        //NetworkObject newNetworkObject =
        //    newOutfit.GetComponent<NetworkObject>();

        //if (newNetworkObject == null)
        //{
        //    Debug.LogError(
        //        "Outfit prefab needs a NetworkObject."
        //    );

        //    Destroy(newOutfit);
        //    return;
        //}

        //newNetworkObject.Spawn();

        //currentOutfit = newOutfit;
    }
}