using Unity.Netcode;
using UnityEngine;

public class SkillRange : Skill
{
    [Header("이펙트")]
    [SerializeField] private GameObject RangeEffect; // 범위 이펙트
    [SerializeField] private float fRangeTime = 3f;   // 범위 유지 시간(소멸까지)
    private float fDamage;
    private float fRadius;

    private float _timer; // 현재 단계 경과 시간(서버 전용)

    public override void Init(SkillType type, SkillData data, Vector3 position, Vector3 direction, ulong clinetID)
    {
        base.Init(type, data, position, direction, clinetID);
        fDamage = data.fDamage;
        fRadius = data.fRadius;
    }

    protected override void OnEnable()
    {
        RangeEffect?.SetActive(true);

        if (!IsServer) return;
        HitOnce();
        _timer = 0f;
    }

    void Update()
    {
        if (!IsServer) return;

        _timer += Time.deltaTime;
        if (_timer >= fRangeTime) EnterDone();
    }

    void EnterDone()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    void HitOnce()
    {
        Collider[] hits = new Collider[NetworkManager.Singleton.ConnectedClientsList.Count];
        Physics.OverlapSphereNonAlloc(transform.position, fRadius , hits, LayerMask.GetMask("Player"));
        foreach (var hit in hits)
            OnHitEnemy(hit, fDamage);
    }

}
