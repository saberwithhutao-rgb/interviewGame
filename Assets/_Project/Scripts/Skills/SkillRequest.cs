using Unity.Netcode;
using UnityEngine;

public struct SkillRequest : INetworkSerializable
{
    public int skillId;
    public Vector3 origin;
    public Vector3 direction;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref skillId);
        serializer.SerializeValue(ref origin);
        serializer.SerializeValue(ref direction);
    }
}