using UnityEngine;

// 충돌한 대상에게 넉백 CC를 적용하는 충돌 이벤트 모듈.
// 넉백 방향 = 대상 위치 - 스킬 위치(충돌 지점에서 밀어냄).
// 서버 권위 — CC 판정/적용은 서버에서만 수행한다.
[System.Serializable]
public class KnockbackOnCollision : ICollsionEventModule
{
    [SerializeField] private CollisionTiming _timing = CollisionTiming.ENTER; // 넉백 판정 시점(진입/겹침)
    [SerializeField] private float _fKnockbackPower;   // 넉백 초기 세기
    [SerializeField] private float _fKnockbackDecay;   // 넉백 감쇠율

    private Skill _skill;

    public CollisionTiming Timing => _timing;

    // 넉백 방향 계산에 쓸 시전 스킬 주입 (프리팹 생성 시 1회)
    public void Bind(Skill skill) => _skill = skill;

    // 충돌 대상에 넉백 적용 (서버 전용)
    public void Collsion(Collider col)
    { 
        if (col.TryGetComponent(out CrowdController crowdController) == false) return;

        Vector3 vKnockbackDir = Vector3.Normalize(col.transform.position - _skill.transform.position);

        crowdController.Apply(CrowdController.CC_TAG.KNOCKBACK,
            new ICrowdControl.CCData(_fKnockbackPower, _fKnockbackDecay, vKnockbackDir));
    }
}
