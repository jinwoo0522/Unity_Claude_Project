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
    public GameObject prefab;

    // Q/마우스 스킬 발동용: 쿨타임(초)·Animator Skill Layer 스테이트명
    [SerializeField] private float  _fCooldown;
    [SerializeField] private string _strSkillState;
    public float  fCooldown     => _fCooldown;
    public string strSkillState => _strSkillState;
}
