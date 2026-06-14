using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Golem 전용 스킬.
// 마우스 스킬(slot 1, Standing Melee Run Jump Attack)에 루트모션 적용.
// applyRootMotion은 항상 false — 루트모션을 서버 Update에서 직접 CCT에 적용해 위치 권위 유지.
public class Golem_Skill : Player_Skill
{
    [SerializeField] private GameObject[] Q_Effects; // Q 스킬 이펙트들 (슬롯 0)
    [SerializeField] private GameObject[] M_Effects; // 마우스 스킬

    [SerializeField] private Transform AxePoint;
    [SerializeField] private Transform BottomPoint;

    private List<ParticleSystem> _qEffectParticles = new List<ParticleSystem>();
    private List<ParticleSystem> _mEffectParticles = new List<ParticleSystem>();
    private CharacterController cct;

    private Player_TrailEmitting _trailEmitter; // 마우스 스킬 전용 트레일 이펙터 (루트모션 이동과 시각적 연동 위해 스킬 클래스에서 직접 제어)
    private bool                _rootMotionActive;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        cct = GetComponent<CharacterController>();
        _trailEmitter = GetComponent<Player_TrailEmitting>();

        // 이펙트 ParticleSystem 컴포넌트 캐싱 — 재생 시마다 GetComponent 호출 방지
        foreach (var effect in Q_Effects)
        {
            var ps = effect.GetComponent<ParticleSystem>();
            if (ps != null) _qEffectParticles.Add(ps);
        }
        foreach (var effect in M_Effects)
        {
            var ps = effect.GetComponent<ParticleSystem>();
            if (ps != null) _mEffectParticles.Add(ps);
        }
    }

    // 마우스 스킬(slot 1)만 루트모션 활성
    protected override void OnSkillStart(int slot)
    {
        // 루트모션 키기 
        _rootMotionActive = (slot == 1);
    }

    protected override void OnSkillEnd()
    {
        _rootMotionActive = false;
    }

    protected override void Update()
    {
        base.Update(); // 스킬 종료 감지
        ApplyRootMotion();
    }

    // 서버에서만 루트모션 소비 — 위치는 NetworkTransform으로 전 클라에 전파
    private void ApplyRootMotion()
    {
        if (!IsServer || !_rootMotionActive || cct == null) return;
        cct.Move(anim.deltaPosition);
    }

    protected override void OnBuff()
    {
        if (!IsOwner) return;
        if(_stat._isDead == true) return;

        SetCooldownLength(SkillType.FireExplosion, 0);
    }


    protected override void OnAttack_Skill()
    {
        if(_stat._isDead == true) return;
        
        if(CheckCanUseSkill(SkillType.FireExplosion, 1) == null)
            return;

        if (IsOwner == true)
        {
            Mouse_EffectToggle_ServerRpc();
            Animation_Play_ServerRpc(SkillType.FireExplosion , 1);
            SetCooldownLength(SkillType.FireExplosion, 1);
        }
        
    }

    void AttackSkillEffectToggle(bool isPlay) //  마우스 스킬 공격 타이밍에 이펙트 토글
    {
        foreach (var effect in _mEffectParticles)
        {    
            if (isPlay)
                effect.Play();
            else
                effect.Stop();
        }

        foreach (var effect in M_Effects)
        {
            effect.SetActive(isPlay);
        }
    }


    void OnAttackSkillAnimationEnd() // 애니메이션 이벤트 — 마우스 스킬 애니메이션 종료 시점에 트레일 이펙트 종료
    {
        Debug.Log("Attack Skill Animation End Event Triggered");
        
        if (IsOwner)
        { 
            Vector3 spawnPosition = AxePoint.position;
            spawnPosition.y = BottomPoint.position.y; // Y 좌표를 BottomPoint의 높이로 고정
            UseSkill_ServerRpc(SkillType.FireExplosion, spawnPosition);
        }

        if (IsServer)
        {
            AttackSkillEvent_ClientRpc();
            _trailEmitter.Stop_EmitTrail_ClientRpc();
        }      
    }
    [ServerRpc]
    void  Mouse_EffectToggle_ServerRpc() // 서버에서 이펙트 토글 명령 수신
    {
        AttackSkill_EffectToggle_ClientRpc();
        _trailEmitter.Start_EmitTrail_ClientRpc();
    }

    [ClientRpc] 
    void AttackSkill_EffectToggle_ClientRpc() // 어택 시작시 이펙트 토글
    {
        AttackSkillEffectToggle(true);
    }

      [ClientRpc]
    void AttackSkillEvent_ClientRpc() // 어택 종료 시 이펙트 토글
    {
        AttackSkillEffectToggle(false);
    }
}
