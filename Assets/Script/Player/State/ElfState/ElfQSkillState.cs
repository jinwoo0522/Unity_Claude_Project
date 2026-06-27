using UnityEngine;

public class ElfQSkillState : EntityState
{
    
    EntityAnimator _aniController;
    IEntityMovement _move;
    IEntityInputState _input;
    IEffector _effector;
    public ElfQSkillState(Elf_Player player)
    {
        _aniController = player._aniController;
        _move = player._move;
        _input = player._input;
        _effector = player._effector;
    }
    public override void Create()
    {
        TransitionList.Add(new QSkillToIdle_Elf(_input));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ELF.StateType.Q_SKILL;
        _effector.PlayEffect((int)ELF.ElfEffect.QSKILL);
    }

    public override void Exit()
    {
        _effector.StopEffect((int)ELF.ElfEffect.QSKILL);
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _move.Gravity();
    }

}
