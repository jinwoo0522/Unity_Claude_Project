using UnityEngine;

public class ElfUpperAttackMiddleState : EntityState
{
    EntityAnimator _upperAniController;
    IEntityInputState _input;
    IHitter _hitter;
    IEntityMovement _move;
    IEffector _effector;
    Stat _stat;

    Vector3 vCentor = new Vector3(0f,1f,0.5f);
    Vector3 vHalfExtents = new Vector3(0.5f,0.25f,0.25f);

    const float fDashSpeed = 15f;
    const float fDashDistance = 2f;
    const float fHitDuration = 0.3f;   // 판정 지속시간(초)
    const float fKnockbackPower = 12f;
    const float fKnockbackDecay = 12f;


    public ElfUpperAttackMiddleState(Elf_Player player)
    {
        _upperAniController = player._upperAniController;
        _input = player._input;
        _hitter = player._hitter;
        _move = player._move;
        _effector = player._effector;
        _stat = player._stat;
    }
    public override void Create()
    {
        TransitionList.Add(new AttackMiddleToLast_Elf(_input, _upperAniController));
        TransitionList.Add(new StateToIdle_Player(_upperAniController , 1f));
        StateEvents.Add((0.25f , EventFunc));
        StateEvents.Add((0.6f , () => _effector.StopTrail((int)ELF.ElfTrail.WEAPON_TRAIL)));
        StateEvents.Add((0.6f , ()=> _effector.StopEffect((int)ELF.ElfEffect.WEAPON_PARTICLE)));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)ELF.UpperStateType.ATTACK_MIDDLE;
        _effector.PlayTrail((int)ELF.ElfTrail.WEAPON_TRAIL);
        _effector.PlayEffect((int)ELF.ElfEffect.WEAPON_PARTICLE);
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
        hitInfo.Target.Hit(_stat.Get_Stat(Stat.STAT_TAG.DAMAGE));
        _effector.PlayEffect((int)ELF.ElfEffect.HIT_EFFECT , hitInfo.Point);
        
        Vector3 vKnocbackDir = Vector3.Normalize(hitInfo.Point - _stat.gameObject.transform.position);
        
        if(hitInfo.Collider.TryGetComponent(out CrowdController crowdController)== true)
            crowdController.Apply(CrowdController.CC_TAG.KNOCKBACK , new ICrowdControl.CCData(fKnockbackPower, fKnockbackDecay, vKnocbackDir));
    }
}
