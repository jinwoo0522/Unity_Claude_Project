using Unity.Netcode;
using UnityEngine;

// 직선 발사체 스킬 — 이동, 파티클 이펙트 전환, 풀 반환 담당
public class SkillProjectile : Skill
{
    [Header("이펙트")]
    public GameObject projectileEffect; // 비행 중 이펙트
    public GameObject hitEffect;        // 충돌 시 이펙트

    private Vector3        vMoveDir;
    private float          fCurrentSpeed; // 현재 속도 (초기값=fSpeed, 매 프레임 fAcceleration 누적)
    private float          fElapsed;      // 비행 경과 시간 (수명 초과 감지용)
    private float          fHitElapsed;   // 히트 이펙트 활성화 후 경과 시간 (첫 프레임 레이스 방지)
    private ParticleSystem hitParticle;   // hitEffect의 파티클 시스템 캐시

    private float fDamage;

    private Vector3 prePos;

    private float fRadius;

    // Awake에서 캐싱 — 서버뿐 아니라 모든 피어에서 OnEnable 전에 1회 실행되므로,
    // Init(서버 전용)을 기다리지 않아도 hitParticle이 항상 유효
    void Awake()
    {
        hitParticle = hitEffect.GetComponent<ParticleSystem>();
    }

    // 발사체 전용 초기화 — 방향/속도 설정 후 base 호출 (서버에서만 호출)
    public override void Init(SkillType type, SkillData data, Vector3 position, Vector3 direction, ulong clinetID)
    {
        base.Init(type, data, position, direction, clinetID);
        vMoveDir      = direction.normalized;
        fCurrentSpeed = data.fSpeed;
        fRadius       = data.fRadius;
        fDamage       = data.fDamage;
    }

    // 재사용 시 발사체 상태 초기화 — Data 여부와 무관하게 항상 리셋
    // (클라 인스턴스는 Init이 호출되지 않아 Data==null이지만 시각 리셋은 반드시 필요)
    protected override void OnEnable()
    {
        base.OnEnable();

        fElapsed    = 0f;
        fHitElapsed = 0f;

        prePos = transform.position;
        projectileEffect.SetActive(true);
        hitEffect.SetActive(false);
        hitParticle?.Clear();
    }

    void Update()
    {
        CheckDespawn();
    }

    void FixedUpdate()
    {
        if (IsServer == false) return;

        ProjectileMove();
        OnHit();
        
    }

    void OnHit()
    {
        if (bHitShown) return;
    
        Vector3 dist = transform.position - prePos;
        RaycastHit output; 
        if(Physics.SphereCast(prePos , fRadius, Vector3.Normalize(dist) , out output , dist.magnitude))
        {
            if(output.collider.CompareTag("Player"))
            {
                // 같은 클라의 발사체가 자신에게 데미지를 주는 경우 방지
                if(output.collider.GetComponent<NetworkObject>().OwnerClientId == GetComponent<NetworkObject>().OwnerClientId)
                    return;

                Player_UpperBody targetUpper = output.collider.GetComponent<Player_UpperBody>();
                if (targetUpper != null) targetUpper.TakeHit(fDamage);
                
            }
            
            OnHit_ClientRpc();
        }
    }
    
    void CheckDespawn()
    {
        // 히트 이펙트 파티클 종료 감시 (첫 프레임 레이스 방지: 0.05s 후부터 체크)
        if(IsOwner == true)
        {
            if (bHitShown == true)
            {
                fHitElapsed += Time.deltaTime;

                 if (fHitElapsed > 0.05f && hitParticle != null && hitParticle.IsAlive(true) == false)
                 {
                    DespawnSkill_ServerRpc();
                 }
            }
        }
    }


    void ProjectileMove()
    {
        if (Data == null) return;

        if (!bHitShown)
        {
            // 가속 이동 및 수명 감시
            fCurrentSpeed += Data.fAcceleration * Time.deltaTime;
            prePos = transform.position; // 이전 위치 저장
            transform.position += vMoveDir * fCurrentSpeed * Time.deltaTime;
            fElapsed += Time.deltaTime;
            if (fElapsed >= Data.fLifeTime) OnHit_ClientRpc();
        }
    }

    // 충돌 또는 수명 만료 시 이펙트 전환, 이후 파티클 수명으로 풀 반환 결정
    protected override void ShowHit()
    {
        if(bHitShown == true)
            return;
        base.ShowHit(); // bHitShown = true
        fHitElapsed = 0f;
        projectileEffect.SetActive(false);
        hitEffect.SetActive(true);
    }

    [ClientRpc]
    void OnHit_ClientRpc()
    {
        ShowHit();
    }
    [ServerRpc]
    void DespawnSkill_ServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        
    }
}
