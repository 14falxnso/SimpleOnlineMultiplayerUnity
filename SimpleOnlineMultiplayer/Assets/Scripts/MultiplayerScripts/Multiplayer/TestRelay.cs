using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    public async Task<string> CreateRelay()
    {
        try
        {
            // 3 connections + host = maximaal 4 spelers
            Allocation allocation =
                await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode =
                await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

            NetworkManager.Singleton.StartHost();

            Debug.Log("Relay created. Join Code: " + joinCode);

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Create Relay failed: " + e);
            return null;
        }
    }

    public async Task JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation =
                await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton
                .GetComponent<UnityTransport>()
                .SetRelayServerData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();

            Debug.Log("Joined Relay with code: " + joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Join Relay failed: " + e);
        }
    }

    public void LeaveRelay()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}