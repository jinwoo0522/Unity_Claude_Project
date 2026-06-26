using UnityEngine;

public class ElfUpperAttackStartState : EntityState
{
    EntityAnimator _upperAniController;
    IEntityInputState _input;
    PlayerMovement _move;
    IHitter _hitter;
    IEffector _effector;

    Vector3 vCentor = new Vector3(0f,1f,0.9f);
    Vector3 vHalfExtents = new Vector3(0.25f,0.25f,0.5f);

    float fDashSpeed = 20f;
    float fDashDistance = 3f;
    float fHitDuration = 0.2f;   // 판정 지속시간(초)

    public ElfUpperAttackStartState(Elf_Player player)
    {
        _upperAniController = player._upperAniController;
        _input = player._input;
        _hitter = player._hitter;
        _move = player._move;
        _effector = player._effector;
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
    }

    public override void Exit()
    {
        ;
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        
    }

    void EventFunc()
    {
        _move.Dash(fDashSpeed , fDashDistance);
        _hitter.DoHitCheck(vCentor , vHalfExtents , fHitDuration);
    }
}
