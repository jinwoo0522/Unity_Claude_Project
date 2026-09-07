using UnityEngine;

// 충돌 시 데미지를 입히는 모듈 — 일발성(진입 1회) / 지속성(주기마다) 선택
// 판정 대상·Owner 제외·서버 권위는 SkillHitCast.CheckLayer가 보장 → 여기선 데미지만 담당
[System.Serializable]
public class SkillDamagable : ISkillModule
{
    // 데미지 방식 — 일발성 / 지속성
    private enum DamageMode { ONE_SHOT, PERSISTENT }

    [SerializeField] private DamageMode _mode;
    [SerializeField] private float _fBaseDamage;      // 기본 데미지 — Owner 공격력에 가산
    [SerializeField] private float _fDamageInterval;  // 지속성 — 데미지 주기(초)

    private Skill _skill;
    private Stat _ownerStat;
    private float _fNextDamageTime;   // 다음 데미지 허용 시각
    private int _iDamageFrame = -1;   // 데미지가 허용된 프레임 — 같은 프레임 다수 대상 처리용

    public void Bind(Skill skill)
    {
        _skill = skill;
    }

    // 발동 — Owner의 Stat 캐싱(Init에서 Owner 확정 뒤 호출됨) + 지속성 상태 초기화
    public void Enter()
    {
        _ownerStat = _skill.Owner.GetComponent<Stat>();
        _fNextDamageTime = 0f;
        _iDamageFrame = -1;
    }

    // 일발성 — 진입 즉시 1회 데미지
    public void CollisionEnter(Collider collider)
    {
        if (_mode != DamageMode.ONE_SHOT) return;

        if (collider.gameObject.TryGetComponent(out IDamagable target) == false) return;

        // [디버그] 실제 Hit 순간 — 어느 스킬이 / 누구 소유로 / 누구를 때리는지 못박음
        Debug.Log($"[SkillDamagable:{_skill.name}] Hit! target={collider.name} targetRoot={collider.transform.root.name} skillOwner={(_skill.Owner == null ? "null" : _skill.Owner.name)}");

        target.Hit(new IDamagable.DamageInfo{
            Damage = FinalDamage(), Attacker = _skill.Owner.transform, Point = collider.ClosestPoint(_skill.transform.position)});
    }

    // 지속성 — CollisionStay는 서버 전용(CheckLayer 보장). 주기 도달 프레임의 겹친 대상 전원에 데미지
    public void CollisionStay(Collider collider)
    {
        if (_mode != DamageMode.PERSISTENT) return;

        // 데미지 프레임 판정은 프레임당 1회 — 여기서 다음 주기를 예약해야 같은 프레임 다른 대상도 통과
        if (_iDamageFrame != Time.frameCount && Time.time >= _fNextDamageTime)
        {
            _iDamageFrame = Time.frameCount;
            _fNextDamageTime = Time.time + _fDamageInterval;
        }

        if (_iDamageFrame != Time.frameCount) return;   // 이번 프레임은 데미지 프레임 아님

        if (collider.gameObject.TryGetComponent(out IDamagable target) == false) return;

        // [디버그] 실제 Hit 순간 — 어느 스킬이 / 누구 소유로 / 누구를 때리는지 못박음
        Debug.Log($"[SkillDamagable:{_skill.name}] (Stay) Hit! target={collider.name} targetRoot={collider.transform.root.name} skillOwner={(_skill.Owner == null ? "null" : _skill.Owner.name)}");

        target.Hit(new IDamagable.DamageInfo{
            Damage = FinalDamage(), Attacker = _skill.Owner.transform, Point = collider.ClosestPoint(_skill.transform.position)});
    }

    // 최종 데미지 = Owner 현재 공격력 + 기본 데미지
    private float FinalDamage()
    {
        return _ownerStat.Get_Stat(Stat.STAT_TAG.DAMAGE) + _fBaseDamage;
    }
}
