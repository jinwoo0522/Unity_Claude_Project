using Unity.Netcode;
using UnityEngine;

// 모든 스킬의 추상 기반 — 공통 데이터, 초기화, 데미지 처리 담당
public abstract class Skill : NetworkBehaviour , ISkill
{

    public SkillType Type { get; private set; }
    public SkillData Data { get; private set; }

    [SerializeField]
    protected IEffector _effector;
    protected IHitter _hitbox;

    protected ulong  ClinetID;

    

    // 공통 초기화 — SetActive는 SkillPool.Get()에서 호출해 OnEnable 타이밍을 제어
    public virtual void Init(SkillType type, SkillData data, Vector3 position, Vector3 direction, ulong clinetID)
    {
        Type               = type;
        Data               = data;
        ClinetID           = clinetID;
        transform.position = position;
        transform.forward  = direction.normalized;
    }

    public virtual void Active()
    {
        gameObject.SetActive(true);
    }

    public virtual void Release()
    {
        gameObject.SetActive(false);
    }

    public virtual void Destroy()
    {
        GameObject.Destroy(gameObject);
    }

    // 풀 생성·재사용 시 공통 리셋 — bHitShown은 Data 없이도 항상 초기화
    protected virtual void OnEnable()
    {
    }

    [ServerRpc]
    protected virtual void DespawnSkill_ServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }


}
