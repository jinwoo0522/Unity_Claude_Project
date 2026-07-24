using Unity.Netcode;
using UnityEngine;

// 발동 후 지정 수명이 지나면 스킬(NetworkObject)을 디스폰해 풀로 반납하는 모듈.
// 서버 권위 — ServerTick은 서버에서만 호출되며, NetworkPoolHandler.Destroy를 통해 풀로 돌아간다.
[System.Serializable]
public class ReturnToPoolOnTime : ISkillModule
{
    [SerializeField] private float _fLifetime = 3f;   // 반납까지 수명(초)

    private Skill _skill;
    private float _fTimer;   // 반납까지 남은 시간

    public void Bind(Skill skill)
    {
        _skill = skill;
    }

    // 발동 — 수명 타이머 초기화
    public void Enter()
    {
        _fTimer = _fLifetime;
    }

    // 서버 권위 — 수명 소진 시 디스폰하여 풀로 반납
    public void ServerTick(float fTimeDelta)
    {
        _fTimer -= fTimeDelta;
        if (_fTimer > 0f) return;

        NetworkObject networkObject = _skill.NetworkObject;
        if (networkObject.IsSpawned == false) return;   // 이미 반납됐다면 중복 디스폰 방지

        networkObject.Despawn();   // NetworkPoolHandler.Destroy → 풀 반납
    }
}
