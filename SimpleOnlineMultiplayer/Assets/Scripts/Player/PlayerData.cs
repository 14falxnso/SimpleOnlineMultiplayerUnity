using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientId;
    public FixedString64Bytes playerName;

    public PlayerData(ulong clientId, string playerName)
    {
        this.clientId = clientId;
        this.playerName = playerName;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
    }

    public bool Equals(PlayerData other)
    {
        return clientId == other.clientId;
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return clientId.GetHashCode();
    }
}