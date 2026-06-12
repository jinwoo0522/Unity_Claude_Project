using Unity.Netcode;
using UnityEngine;

// 범위형 스킬 — Cast(캐스팅) → Range(범위/데미지) → Done(소멸) 단방향 상태머신.
// 수명·데미지 판정은 서버에서만 수행(서버 권위적). 클라는 이펙트 토글만 동기화한다.
public class SkillCastRange : Skill
{
    [Header("이펙트")]
    [SerializeField] private GameObject RangeEffect; // 범위 이펙트
    [SerializeField] private GameObject CastEffect;  // 캐스팅 이펙트

    [Header("단계별 지속시간(초)")]
    [SerializeField] private float fCastTime  = 0.5f; // 캐스팅 유지 시간
    [SerializeField] private float fRangeTime = 3f;   // 범위 유지 시간(소멸까지)

    // 스킬 진행 단계 — 단방향 전이만 허용해 재진입/반복을 원천 차단
    private enum Phase { Cast, Range, Done }
    private Phase _phase;
    private float _timer; // 현재 단계 경과 시간(서버 전용)

    private float fDamage;
    private float fRadius;

    public override void Init(SkillType type, SkillData data, Vector3 position, Vector3 direction, ulong clinetID)
    {
        base.Init(type, data, position, direction, clinetID);
        fDamage = data.fDamage;
        fRadius = data.fRadius;
    }

    // 풀 재사용 시 리셋 — 각 클라 인스턴스에서 OnEnable이 호출되므로 초기 이펙트도 로컬로 설정
    protected override void OnEnable()
    {
        base.OnEnable();
        _phase = Phase.Cast;
        _timer = 0f;
        CastEffect?.SetActive(true);
        RangeEffect?.SetActive(false);
    }

    // 수명 판정은 서버 전용 — 시간 경과로 단계 전환
    void Update()
    {
        if (!IsServer) return;

        _timer += Time.deltaTime;

        switch (_phase)
        {
            case Phase.Cast:
                if (_timer >= fCastTime) EnterRange();
                break;

            case Phase.Range:
                if (_timer >= fRangeTime) EnterDone();
                break;
        }
    }

    // 캐스팅 종료 → 범위 단계 진입(이펙트 전환 + 1회 데미지 판정)
    void EnterRange()
    {
        _phase = Phase.Range;
        _timer = 0f;
        SetEffect_ClientRpc(false, true);
        DealDamageOnce();
    }

    // 범위 종료 → 소멸. 서버에서 바로 Despawn → 풀 반환
    void EnterDone()
    {
        _phase = Phase.Done;
        GetComponent<NetworkObject>().Despawn();
    }

    // 서버 1회 범위 데미지 판정
    void DealDamageOnce()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, fRadius);
        foreach (var hit in hits)
            OnHitEnemy(hit, fDamage, isHitAnim);
    }

    // 이펙트 켜고 끄기만 전 클라(호스트 포함) 동기화 — 수명 판정 아님
    [ClientRpc]
    void SetEffect_ClientRpc(bool cast, bool range)
    {
        CastEffect?.SetActive(cast);
        RangeEffect?.SetActive(range);
    }
}
