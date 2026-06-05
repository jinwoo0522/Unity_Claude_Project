using Unity.Netcode;
using UnityEngine;

public class SkillRange : Skill
{
    [Header("이펙트")]
    public GameObject RangeEffect;  // 범위 이펙트
    public GameObject CastEffect;   // 캐스팅 이펙트


    private ParticleSystem castParticle; // CastEffect의 파티클 시스템 캐시
    private ParticleSystem rangeParticle; // RangeEffect의 파티클 시스템 캐시

     void Awake()
    {
        castParticle = CastEffect.GetComponent<ParticleSystem>();
        rangeParticle = RangeEffect.GetComponent<ParticleSystem>();
    }

    private float fDamage;
    private float fRadius;
    private float fElapsed; // 범위 유지 경과 시간 (수명 초과 감지용)
    private bool isCheckSphareCast; // 범위 내 적 탐지 여부 플래그

    public override void Init(SkillType type, SkillData data, Vector3 position, Vector3 direction, ulong clinetID)
    {
        base.Init(type, data, position, direction, clinetID);
        fDamage = data.fDamage;
        fRadius = data.fRadius;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        RangeEffect?.SetActive(false);
        CastEffect?.SetActive(true);
        fElapsed = 0f; // 범위 유지 시간 측정 시작
        castParticle.Clear();
    }

    void FixedUpdate()
    {
        if(IsServer == true)
        {
            if(isCheckSphareCast == true)
            {
                CastSphere();
            }
        }

    }
    void Update()
    {
        if(IsOwner == true)
        {
            CheckCastOver();
            CheckDespawn();
        }    
    }

    void CastSphere()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, fRadius);
        foreach (var hitCollider in hitColliders)
        {
            OnHitEnemy(hitCollider , fDamage);
        }

        isCheckSphareCast = false; // 한 번 탐지 후 플래그 리셋 (지속적으로 탐지하지 않도록)
    }

    void CheckCastOver()
    {
        if(castParticle == null && RangeEffect.activeSelf == false) // 캐스팅이 없으면 그냥 실행
        {
            RangeEffect.SetActive(true);
            isCheckSphareCast = true; // 지금부터 스페어 캐스트 시작
            fElapsed = 0f;
            return;
        }

        fElapsed += Time.deltaTime;
        if (fElapsed > 0.05f && castParticle.IsAlive(true) == false)
        {
            CastEffect.SetActive(false);
            RangeEffect.SetActive(true);
            ShowRange_ServerRpc();
            fElapsed= 0f;
        }
    }

    void CheckDespawn()
    {
        if (RangeEffect.activeSelf == true)
        {
            fElapsed += Time.deltaTime;
            if (fElapsed > 0.05f && rangeParticle != null && rangeParticle.IsAlive(true) == false)
            {
                DespawnSkill_ServerRpc();
            }
        }
    }

    [ServerRpc]
    void ShowRange_ServerRpc()
    {
        CastEffect.SetActive(false);
        RangeEffect.SetActive(true);
        fElapsed= 0f;
    }

}
