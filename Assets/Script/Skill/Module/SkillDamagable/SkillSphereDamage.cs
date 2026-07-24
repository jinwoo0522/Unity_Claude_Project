using System;
using UnityEngine;

// 구체 캐스트(OverlapSphereNonAlloc)로 범위 내 대상에게 데미지를 입히는 모듈.
// 시점(ENTER/COLLISION/EXIT)에 해당하는 이벤트에만 캐스트 트리거를 연결해 두고 각 시점에서 Invoke 한다.
// 서버 권위 — 캐스트/데미지는 서버에서만 수행한다.
[System.Serializable]
public class SkillSphereDamage : ISkillModule
{
    // 캐스트 시점 — 발동 시 / 히트캐스트 충돌 시 / 종료 시
    private enum CastTiming { ENTER, COLLISION, EXIT }

    [SerializeField] private CastTiming _timing;         // 캐스트 시점
    [SerializeField] private float      _fDelay;         // 시점 트리거 후 캐스트까지 지연(초)
    [SerializeField] private Vector3    _vOffset;        // 캐스트 중심 오프셋 — 스킬 회전을 따라감(로컬)
    [SerializeField] private float      _fRadius = 1f;   // 구 반경
    [SerializeField] private LayerMask  _targetMask;     // 판정 대상 레이어
    [SerializeField] private float      _fBaseDamage;    // 기본 데미지 — Owner 공격력에 가산

    private Skill      _skill;
    private Stat       _ownerStat;
    private Collider[] _hitBuffer;                        // NonAlloc 결과 버퍼 — 재사용으로 GC 회피
    private float      _fTimer = float.NegativeInfinity;  // 캐스트 대기 타이머 — 음수면 비활성

    // 시점별 이벤트 — _timing에 해당하는 하나에만 트리거가 연결되고 나머지는 빈 동작
    private Action _onEnter;
    private Action _onCollision;
    private Action _onExit;

    // _timing에 따라 해당 시점 이벤트에만 캐스트 트리거를 연결 (분기는 여기 1회뿐)
    public void Bind(Skill skill)
    {
        _skill     = skill;
        _hitBuffer = new Collider[16];
        _onEnter   = _onCollision = _onExit = Noop;

        switch (_timing)
        {
            case CastTiming.ENTER:     _onEnter     = Schedule;     break;   // 발동 즉시 캐스트
            case CastTiming.COLLISION: _onCollision = Schedule; break;
            case CastTiming.EXIT:      _onExit      = Cast;     break;   // 종료 후 틱이 없어 즉시 캐스트
        }
    }

    // 발동 — Owner Stat 캐싱 + 타이머 초기화 후 ENTER 이벤트 Invoke
    public void Enter()
    {
        _ownerStat = _skill.Owner.GetComponent<Stat>();
        _fTimer    = float.NegativeInfinity;
        _onEnter.Invoke();
    }

    public void CollisionEnter(Collider collider) => _onCollision.Invoke();

    // 서버 권위 — 예약된 딜레이 경과 시 1회 캐스트
    public void ServerTick(float fTimeDelta)
    {
        if (_fTimer < 0f) return;

        _fTimer -= fTimeDelta;
        if (_fTimer > 0f) return;

        _fTimer = float.NegativeInfinity;
        Cast();
    }

    public void Exit() => _onExit.Invoke();

    // 캐스트 예약 — 딜레이만큼 타이머 설정 (ServerTick이 소진 후 캐스트)
    private void Schedule() => _fTimer = _fDelay;

    private void Noop() { }

    // 구체 캐스트 1회 + 겹친 대상 전원에 데미지 (서버 전용, 시전자 제외)
    private void Cast()
    {
        Vector3 vCenter = _skill.transform.position + _skill.transform.rotation * _vOffset;
        int iCount = Physics.OverlapSphereNonAlloc(vCenter, _fRadius, _hitBuffer, _targetMask);

        for (int i = 0; i < iCount; ++i)
        {
            Collider col = _hitBuffer[i];

            if (col.transform.root == _skill.Owner.transform) continue;   // 시전자 제외
            if (col.TryGetComponent(out IDamagable target) == false) continue;

            target.Hit(FinalDamage());
        }
    }

    // 최종 데미지 = Owner 현재 공격력 + 기본 데미지
    private float FinalDamage() => _ownerStat.Get_Stat(Stat.STAT_TAG.DAMAGE) + _fBaseDamage;
}
