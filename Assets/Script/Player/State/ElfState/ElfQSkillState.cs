using UnityEngine;

public class ElfQSkillState : EntityState
{
    
    EntityAnimator _aniController;
    IEntityMovement _move;
    IEntityRotate _rotate;
    IEntityInputState _input;
    IEffector _effector;
    private StateMachine _upperStateMachine;
    public ElfQSkillState(Elf_Player player)
    {
        _aniController = player._aniController;
        _move = player._move;
        _rotate = player._rotate;
        _input = player._input;
        _effector = player._effector;
        _upperStateMachine = player._upperStateMachine;
    }
    public override void Create()
    {
        TransitionList.Add(new QSkillToIdle_Elf(_input));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ELF.StateType.Q_SKILL;
        _effector.PlayEffect((int)ELF.ElfEffect.QSKILL);
        _upperStateMachine.TransitionTo((ushort)ENTITY.UpperStateType.EMPTY);   // 상체 잠금
    }

    public override void Exit()
    {
        _effector.StopEffect((int)ELF.ElfEffect.QSKILL);
        _upperStateMachine.TransitionTo((ushort)ENTITY.UpperStateType.IDLE);    // 상체 복귀
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _move.Gravity();
        _rotate.Rotate();
    }

}
