using Unity.Netcode;
using UnityEngine;

// 모든 스킬의 추상 기반 — 공통 데이터, 초기화, 데미지 처리 담당
public abstract class Skill : NetworkBehaviour
{
    public SkillType Type { get; private set; }
    public SkillData Data { get; private set; }

    protected ulong  ClinetID;
    protected bool   bHitShown;

    // 공통 초기화 — SetActive는 SkillPool.Get()에서 호출해 OnEnable 타이밍을 제어
    public virtual void Init(SkillType type, SkillData data, Vector3 position, Vector3 direction, ulong clinetID)
    {
        Type               = type;
        Data               = data;
        ClinetID           = clinetID;
        transform.position = position;
        transform.forward  = direction.normalized;
    }

    // 풀 생성·재사용 시 공통 리셋 — bHitShown은 Data 없이도 항상 초기화
    protected virtual void OnEnable()
    {
        bHitShown = false;
    }

    // bHitShown 플래그 설정 — 자식에서 이펙트 전환 오버라이드
    protected virtual void ShowHit()
    {
        if (bHitShown) return;
        bHitShown = true;
    }

    // 공통 데미지 계산 — 필요 시 자식에서 오버라이드
    protected virtual void OnHitEnemy(GameObject other)
    {
        // var targetStat = other.GetComponent<Stat>();
        // var ownerStat  = Owner?.GetComponent<Stat>();
        // if (targetStat == null) return;
        // float dmg = Data.fDamage + (ownerStat != null ? ownerStat.pDamage : 0f);
        // targetStat.pHp = -dmg;
    }
}
