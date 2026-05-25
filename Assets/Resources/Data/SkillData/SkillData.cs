using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public string strName;
    public float fSpeed        = 10f; // 초기 속도
    public float fAcceleration = 5f;  // 초당 속도 증가량 (0이면 등속)
    public float fLifeTime     = 3f;
    public float fDamage       = 10f; // 추후 Enemy 시스템 연결
    public int   nPoolSize     = 5;
    public GameObject prefab;
}
