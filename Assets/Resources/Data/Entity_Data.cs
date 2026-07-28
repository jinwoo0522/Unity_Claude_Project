using UnityEngine;
public class Entity_Data : ScriptableObject
{
    [Header("이름")]
    public string strName;
    [TextArea]
    public string strDesc;

    [Header("공격")]
    public float fAttackDamage = 3f;
    
    [Header("방어")]
    public float fMaxHp = 100f;    
    [Header("이동")]
    public float fWalkSpeed = 3f;
    public float fRunSpeed = 6f;
    public float fRotateSpeed = 10f;   // 회전 보간 속도 — 조준으로 즉시 도는 플레이어는 사용하지 않는다
    public float fGravity = -9.8f;     // 모든 엔티티가 같은 규칙으로 낙하하므로 공통으로 둔다

    [Header("탐지")]
    public float fDetectRange = 10f;      // 타겟을 잡는 사거리
    public float fReturnDistance = 15f;   // 스폰 지점에서 이 거리를 넘으면 복귀한다

    [Header("마나")]
    public float fMaxMana   = 100f;
    public float fManaRegen = 2f;
}
