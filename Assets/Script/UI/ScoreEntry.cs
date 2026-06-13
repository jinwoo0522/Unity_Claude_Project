using System;
using Unity.Collections;
using Unity.Netcode;

// NetworkList 전송용 직렬화 구조체 — 서버 쓰기, 전 클라 읽기
public struct ScoreEntry : INetworkSerializable, IEquatable<ScoreEntry>
{
    public ulong              clientId;
    public FixedString64Bytes name;
    public int                kills;
    public int                deaths;
    public float              damageDealt;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref kills);
        serializer.SerializeValue(ref deaths);
        serializer.SerializeValue(ref damageDealt);
    }

    public bool Equals(ScoreEntry other) =>
        clientId    == other.clientId    &&
        name        == other.name        &&
        kills       == other.kills       &&
        deaths      == other.deaths      &&
        damageDealt == other.damageDealt;
}
