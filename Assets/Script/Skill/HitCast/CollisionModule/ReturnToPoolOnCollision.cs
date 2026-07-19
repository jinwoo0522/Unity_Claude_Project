using Unity.Netcode;
using UnityEngine;

// 충돌 시 스킬(NetworkObject)을 디스폰해 풀로 반납하는 충돌 모듈.
// 서버 권위적 — 디스폰은 서버에서만 수행하고, NetworkPoolHandler.Destroy를 통해 풀로 돌아간다.
[System.Serializable]
public class ReturnToPoolOnCollision : ICollsionEventModule
{
    [SerializeField] private CollisionTiming _timing = CollisionTiming.ENTER; // 반납 판정 시점(진입/겹침)

    private Skill _skill;

    public CollisionTiming Timing => _timing;

    // 반납 대상 스킬 주입 (프리팹 생성 시 1회)
    public void Bind(Skill skill)
    {
        _skill = skill;
    }

    // 충돌 발생 — 서버에서만 디스폰하여 풀로 반납
    public void Collsion(Collider col)
    {
        if (_skill.IsServer == false) return; // 디스폰은 서버 권위

        NetworkObject networkObject = _skill.NetworkObject;
        if (networkObject.IsSpawned == false) return; // 같은 프레임 다중 충돌의 중복 디스폰 방지

        networkObject.Despawn(); // NetworkPoolHandler.Destroy → 풀 반납
    }
}
