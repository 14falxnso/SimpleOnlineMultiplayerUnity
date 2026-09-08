using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : NetworkBehaviour
{
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>
        (
        new MyCustomData
        {
            _int = 0,
            _bool = true,
        }
        , NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner );


    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int );
            serializer.SerializeValue(ref _bool );
        }
    }
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousNumber, MyCustomData newValue) =>{

        };
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;  
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TestServerRPC();
            //randomNumber.Value = new MyCustomData
            //{
            //    _int = 10,
            //    _bool = false
            //};
        }

        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if(Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if(Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void TestServerRPC()
    {

    }
}
