using System;
using System.Collections.Generic;
using UnityEngine;

// 구체 캐스트(OverlapSphereNonAlloc)를 매 틱 수행해 범위 내 대상을 수집하고,
// 주입된 충돌 이벤트 모듈(_onHit)의 Collsion을 그 Timing에 맞는 이벤트에 등록해 실행하는 모듈.
//   - Timing.ENTER : 첫 캐스트(진입) 시 1회
//   - Timing.STAY  : 캐스트마다(겹친 동안 매 틱)
// 레이어/시전자 제외는 여기서 처리하고, 실제 판정(데미지/넉백 등)은 모듈에 위임한다.
// 서버 권위 — 캐스트/판정은 서버(ServerTick)에서만 수행한다.
[System.Serializable]
public class SkillSphereCast : ISkillModule
{
    [SerializeField] private float     _fDelay;         // 발동 후 캐스트 시작까지 지연(초, 윈드업)
    [SerializeField] private Vector3   _vOffset;        // 캐스트 중심 오프셋 — 스킬 회전을 따라감(로컬)
    [SerializeField] private float     _fRadius = 1f;   // 구 반경
    [SerializeField] private LayerMask _targetMask;     // 판정 대상 레이어
    // 캐스트로 걸린 대상에 실행할 충돌 이벤트 모듈 조합 (데미지/넉백 등)
    [SerializeReference, SubclassSelector] private List<ICollsionEventModule> _onHit = new List<ICollsionEventModule>();

    private Skill      _skill;
    private Collider[] _hitBuffer;   // NonAlloc 결과 버퍼 — 재사용으로 GC 회피
    private float      _fWindup;     // 남은 윈드업 시간 — 0 이하가 되면 캐스트 시작
    private bool       _bEntered;    // 첫 캐스트 완료 여부 — ENTER 이벤트를 첫 캐스트에서만 발생

    // 모듈의 Timing에 맞춰 Collsion을 등록 (SkillHitCast와 동일한 방식)
    private Action<Collider> _hitEnterEvent;
    private Action<Collider> _hitStayEvent;

    // 주입 모듈을 Bind하고 Timing별 이벤트에 Collsion 등록 (프리팹 생성 시 1회)
    public void Bind(Skill skill)
    {
        _skill     = skill;
        _hitBuffer = new Collider[16];

        for (int i = 0; i < _onHit.Count; ++i)
        {
            _onHit[i].Bind(skill);

            if (_onHit[i].Timing == CollisionTiming.ENTER)
                _hitEnterEvent += _onHit[i].Collsion;
            else
                _hitStayEvent += _onHit[i].Collsion;
        }
    }

    // 발동 — 윈드업/진입 상태 초기화 (풀 재사용 대비)
    public void Enter()
    {
        _fWindup  = _fDelay;
        _bEntered = false;
    }

    // 서버 권위 — 윈드업 경과 후 매 틱 캐스트
    public void ServerTick(float fTimeDelta)
    {
        if (_fWindup > 0f)
        {
            _fWindup -= fTimeDelta;
            return;
        }

        Cast();
    }

    // 구체 캐스트 1회 + 걸린 대상마다 Timing 이벤트 발생 (레이어/시전자 제외는 여기서)
    private void Cast()
    {
        Vector3 vCenter = _skill.transform.position + _skill.transform.rotation * _vOffset;
        int iCount = Physics.OverlapSphereNonAlloc(vCenter, _fRadius, _hitBuffer, _targetMask);

        for (int i = 0; i < iCount; ++i)
        {
            Collider col = _hitBuffer[i];

            if (col.transform.root == _skill.Owner.transform) continue;   // 시전자 제외

            if (_bEntered == false)
                _hitEnterEvent?.Invoke(col);   // 첫 캐스트 = 진입

            _hitStayEvent?.Invoke(col);        // 겹친 동안 매 틱
        }

        _bEntered = true;
    }
}
