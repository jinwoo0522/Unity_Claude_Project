using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public string strName;
    public float fSpeed        = 10f; // 초기 속도
    public float fAcceleration = 5f;  // 초당 속도 증가량 (0이면 등속)
    public float fLifeTime     = 3f;
    public float fDamage       = 10f; // 추후 Enemy 시스템 연결
    public float fRadius       = 1f;
    public int   nPoolSize     = 5;
    public Skill prefab;

    // Q/마우스 스킬 발동용: 쿨타임(초)·Animator Skill Layer 스테이트명
    [SerializeField] private float  _fCooldown;
    [SerializeField] private string _strSkillState;
    public float  fCooldown     => _fCooldown;
    public string strSkillState => _strSkillState;

    // 피격 시 대상에게 가할 넉백 힘 — 0이면 넉백 없음(기존 스킬 동작 유지)
    [SerializeField] private float _fKnockback = 0f;
    public float fKnockback => _fKnockback;

    // 피격 시 대상을 공중으로 띄우는 상승 속도 — 0이면 효과 없음
    [SerializeField] private float _fLaunchForce = 0f;
    public float fLaunchForce => _fLaunchForce;

    // 슬로우 이동 속도 배율 — 1이면 효과 없음(0~1)
    [SerializeField] private float _fSlowMultiplier = 1f;
    public float fSlowMultiplier => _fSlowMultiplier;

    // 슬로우 지속 시간(초) — 0이면 슬로우 없음
    [SerializeField] private float _fSlowDuration = 0f;
    public float fSlowDuration => _fSlowDuration;

    // 빙결 지속 시간(초) — 0이면 빙결 없음
    [SerializeField] private float _fFreezeDuration = 0f;
    public float fFreezeDuration => _fFreezeDuration;

    // 스킬 발동에 필요한 마나 소모량 — 0이면 마나 소모 없음(골렘 기본공격 등)
    [SerializeField] private float _fManaCost = 0f;
    public float fManaCost => _fManaCost;
}
