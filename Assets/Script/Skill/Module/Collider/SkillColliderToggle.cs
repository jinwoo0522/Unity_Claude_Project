using UnityEngine;

// 발동(Enter) 후 지정 시간이 지나면 히트 콜라이더를 켜고, 종료(Exit) 시 끈다 — 판정 지연용
[System.Serializable]
public class SkillColliderToggle : ISkillModule
{
    [SerializeField] private float _fEnableDelay = 0f;   // 콜라이더 활성 지연(초) — 0이면 Enter 즉시
    private Collider _collider;
    private float    _fTimer;                            // 활성까지 누적 시간 — 활성 완료 시 -1(래치)

    // 시전 스킬의 히트캐스트 콜라이더를 캐시하고 초기엔 꺼둔다 (활성 전까지 판정 차단)
    public void Bind(Skill skill)
    {
        _collider = skill.Hitcast.GetComponent<Collider>();
        _collider.enabled = false;
    }

    public void Enter()
    {
        _collider.enabled = false;
        _fTimer = 0f;
    }

    // 서버 권위 — 지연 경과 시 1회 콜라이더 활성 (0초면 첫 틱에 활성)
    public void ServerTick(float fTimeDelta)
    {
        if (_fTimer < 0f) return;

        if (_fTimer < _fEnableDelay)
        {
            _fTimer += fTimeDelta;
            return;
        }

        _fTimer = -1f;
        _collider.enabled = true;
    }

    public void Exit()
    {
        _collider.enabled = false;
    }
}
