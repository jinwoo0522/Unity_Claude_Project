using Unity.Netcode;
using UnityEngine;

// 모든 스킬의 추상 기반 — 공통 데이터, 초기화, 데미지 처리 담당
public abstract class Skill : NetworkBehaviour
{
    [SerializeField] protected bool isHitAnim = true; // 피격 시 애니메이션 재생 여부
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
    protected virtual void OnHitEnemy(Collider output , float fDamage , bool isHitAni = true)
    {
        if(output.CompareTag("Player"))
        {
            // 같은 클라의 발사체가 자신에게 데미지를 주는 경우 방지
            if(output.GetComponent<NetworkObject>().OwnerClientId == GetComponent<NetworkObject>().OwnerClientId)
                return;

            Player_UpperBody targetUpper = output.GetComponent<Player_UpperBody>();
            if (targetUpper != null)
            {
                targetUpper.TakeHit(fDamage , isHitAni);
                // fKnockback > 0 가드: 0인 스킬이 진행 중인 넉백을 강제로 0으로 덮어쓰는 버그 방지
                if (Data.fKnockback > 0f)
                {
                    Vector3 dir = output.transform.position - transform.position;
                    output.GetComponent<Player_Move>().ApplyKnockback(dir, Data.fKnockback);
                }

                Status_Effect(output);

            }
        }
    }

    void Status_Effect(Collider output)
    {
        // 공중 띄움 — fLaunchForce > 0인 스킬에서만 발동
        if (Data.fLaunchForce > 0f)
            output.GetComponent<Player_Status>().ApplyAirborne(Data.fLaunchForce);
        // 슬로우 — fSlowDuration > 0인 스킬에서만 발동
        if (Data.fSlowDuration > 0f)
            output.GetComponent<Player_Status>().ApplySlow(Data.fSlowMultiplier, Data.fSlowDuration);
        // 빙결 — fFreezeDuration > 0인 스킬에서만 발동
        if (Data.fFreezeDuration > 0f)
            output.GetComponent<Player_Status>().ApplyFreeze(Data.fFreezeDuration);
    }

    [ServerRpc]
    protected virtual void DespawnSkill_ServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }
}
