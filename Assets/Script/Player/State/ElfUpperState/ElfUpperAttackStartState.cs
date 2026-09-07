using UnityEngine;

public class ElfUpperAttackStartState : EntityState
{
    EntityAnimator _upperAniController;
    IEntityInputState _input;
    IEntityMovement _move;
    IHitter _hitter;
    IEffector _effector;
    Stat _stat;
    MotionTrailer _motionTrailer;
    Vector3 vCentor = new Vector3(0f,1f,0.9f);
    Vector3 vHalfExtents = new Vector3(0.25f,0.25f,0.5f);

    const float fDashSpeed = 20f;
    const float fDashDistance = 3f;
    const float fHitDuration = 0.2f;   // 판정 지속시간(초)
    const float fKnockbackPower = 15f;
    const float fKnockbackDecay = 12f;

    public ElfUpperAttackStartState(Elf_Player player)
    {
        _upperAniController = player._upperAniController;
        _input = player._input;
        _hitter = player._hitter;
        _move = player._move;
        _effector = player._effector;
        _stat = player._stat;
        _motionTrailer = player._MotionTrailer;
    }
    public override void Create()
    {
        TransitionList.Add(new AttackStartToMiddle_Elf(_input, _upperAniController ));
        TransitionList.Add(new StateToIdle_Player(_upperAniController , 1f));
        StateEvents.Add((0.4f , EventFunc));
        StateEvents.Add((0.7f , ()=> _effector.StopTrail((int)ELF.ElfTrail.WEAPON_TRAIL)));
        StateEvents.Add((0.7f , ()=> _effector.StopEffect((int)ELF.ElfEffect.WEAPON_PARTICLE)));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)ELF.UpperStateType.ATTACK_START;
        _effector.PlayTrail((int)ELF.ElfTrail.WEAPON_TRAIL);
        _effector.PlayEffect((int)ELF.ElfEffect.WEAPON_PARTICLE);
        _motionTrailer.Play_Trail(1f);
    }

    public override void Exit()
    {
        _effector.StopEffect((int)ELF.ElfEffect.HIT_EFFECT);
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        
    }

    void EventFunc()
    {
        _move.Dash(fDashSpeed , fDashDistance);
        _hitter.DoHitCheck(vCentor , vHalfExtents , fHitDuration, HitHandler);
    }

    void HitHandler(IHitter.HitInfo hitInfo)
    {
        hitInfo.Target.Hit(new IDamagable.DamageInfo{
            Damage = _stat.Get_Stat(Stat.STAT_TAG.DAMAGE), Attacker = _stat.transform, Point = hitInfo.Point});
        _effector.PlayEffect((int)ELF.ElfEffect.HIT_EFFECT , hitInfo.Point);

        Vector3 vKnocbackDir = Vector3.Normalize(hitInfo.Point - _stat.gameObject.transform.position);
        
        if(hitInfo.Collider.TryGetComponent(out CrowdController crowdController)== true)
            crowdController.Apply(CrowdController.CC_TAG.KNOCKBACK, 
                new ICrowdControl.CCData(fKnockbackPower, fKnockbackDecay, vKnocbackDir));
    }
}
