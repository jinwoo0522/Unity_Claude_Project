using Unity.Netcode;
using UnityEngine;

// 직선 발사체 스킬 — 이동, 파티클 이펙트 전환, 풀 반환 담당
public class SkillProjectile : Skill
{

    private Vector3        vMoveDir;
    private float          fCurrentSpeed; // 현재 속도 (초기값=fSpeed, 매 프레임 fAcceleration 누적)
    private float          fElapsed;      // 비행 경과 시간 (수명 초과 감지용)
    private float          fHitElapsed;   // 히트 이펙트 활성화 후 경과 시간 (첫 프레임 레이스 방지)
    private ParticleSystem hitParticle;   // hitEffect의 파티클 시스템 캐시

    private float fDamage;

    private Vector3 prePos;

    private float fRadius;


    // 발사체 전용 초기화 — 방향/속도 설정 후 base 호출 (서버에서만 호출)
    public override void Init(SkillType type, SkillData data, Vector3 position, Vector3 direction, ulong clinetID)
    {
        base.Init(type, data, position, direction, clinetID);
        vMoveDir      = direction.normalized;
        fCurrentSpeed = data.fSpeed;
        fRadius       = data.fRadius;
        fDamage       = data.fDamage;
    }

    // 재사용 시 발사체 상태 초기화 — Data 여부와 무관하게 항상 리셋
    // (클라 인스턴스는 Init이 호출되지 않아 Data==null이지만 시각 리셋은 반드시 필요)
    protected override void OnEnable()
    {
        base.OnEnable();
        prePos = transform.position;
    }

    void Update()
    {
        if (IsServer == false) return;
        ProjectileMove();
    }

    void FixedUpdate()
    {

    }


    void ProjectileMove()
    {
        if (Data == null) return;

        // 가속 이동 및 수명 감시
        fCurrentSpeed += Data.fAcceleration * Time.deltaTime;
        prePos = transform.position; // 이전 위치 저장
        transform.position += vMoveDir * fCurrentSpeed * Time.deltaTime;
        fElapsed += Time.deltaTime;
    }

}
