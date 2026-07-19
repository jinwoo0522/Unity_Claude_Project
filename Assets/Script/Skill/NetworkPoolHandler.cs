using UnityEngine;
using Unity.Netcode;

// 스킬 전용 네트워크 풀 — 내부에 순수 풀(PooledHandler)을 소유하고,
// INetworkPrefabInstanceHandler를 구현해 NGO 스폰/디스폰을 풀과 연결한다.
public class NetworkPoolHandler : INetworkPrefabInstanceHandler
{
    private readonly PooledHandler _pool;

    public NetworkPoolHandler(GameObject prefab, int iMinSize, int iMaxSize)
    {
        _pool = new PooledHandler(prefab, iMinSize, iMaxSize);
    }

    // NGO 스폰 요청 → 풀에서 꺼내 위치 지정 후 NetworkObject 반환
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 vPosition, Quaternion qRotation)
    {
        Skill skill = (Skill)_pool.Get();
        skill.transform.SetPositionAndRotation(vPosition, qRotation);
        return skill.GetComponent<NetworkObject>();
    }

    // NGO 디스폰 → 풀에 반납
    public void Destroy(NetworkObject networkObject)
    {
        _pool.Return(networkObject.GetComponent<Skill>());
    }

    // 풀에서 직접 획득 (NGO 스폰 우회 — 내부/특수 용도)
    public IPoolable Get() => _pool.Get();

    public void Prewarm(int iCount) => _pool.Prewarm(iCount);
}
