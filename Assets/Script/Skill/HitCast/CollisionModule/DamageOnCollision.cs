using UnityEngine;

// 충돌한 대상에게 데미지를 입히는 충돌 이벤트 모듈 — 최종 데미지 = Owner 공격력 + 기본 데미지.
// 물리 트리거(SkillHitCast) 또는 구체 캐스트(SkillSphereCast) 어느 쪽이 구동해도 동작한다.
// 서버 권위 — 데미지 판정은 서버에서만 수행한다.
[System.Serializable]
public class DamageOnCollision : ICollsionEventModule
{
    [SerializeField] private CollisionTiming _timing = CollisionTiming.ENTER; // 물리 트리거 구독 시점(캐스트 구동 시 무시)
    [SerializeField] private float _fBaseDamage;   // 기본 데미지 — Owner 공격력에 가산

    private Skill _skill;

    public CollisionTiming Timing => _timing;

    // 데미지 계산에 쓸 시전 스킬 주입 (프리팹 생성 시 1회)
    public void Bind(Skill skill) => _skill = skill;

    // 충돌 대상에 데미지 (서버 전용)
    public void Collsion(Collider col)
    {
        if (_skill.IsServer == false) return;                             // 데미지는 서버 권위
        if (col.TryGetComponent(out IDamagable target) == false) return;

        target.Hit(FinalDamage());
    }

    // 최종 데미지 = Owner 현재 공격력 + 기본 데미지
    private float FinalDamage()
        => _skill.Owner.GetComponent<Stat>().Get_Stat(Stat.STAT_TAG.DAMAGE) + _fBaseDamage;
}
