using UnityEngine;

public class ElfUpperAttackMiddleState : EntityState
{
    EntityAnimator _upperAniController;
    IEntityInputState _input;
    IHitter _hitter;
    PlayerMovement _move;
    IEffector _effector;

    Vector3 vCentor = new Vector3(0f,1f,0.5f);
    Vector3 vHalfExtents = new Vector3(0.5f,0.25f,0.25f);

    float fDashSpeed = 15f;
    float fDashDistance = 2f;

    float fHitDuration = 0.3f;   // 판정 지속시간(초)

    public ElfUpperAttackMiddleState(Elf_Player player)
    {
        _upperAniController = player._upperAniController;
        _input = player._input;
        _hitter = player._hitter;
        _move = player._move;
        _effector = player._effector;
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
