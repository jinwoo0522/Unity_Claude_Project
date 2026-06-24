using UnityEngine;

public class ElfUpperAttackStartState : EntityState
{
    EntityAnimator _upperAniController;
    IEntityInputState _input;

    public ElfUpperAttackStartState(Player player)
    {
        _upperAniController = player._upperAniController;
        _input = player._input;
    }
    public override void Create()
    {
        TransitionList.Add(new AttackStartToMiddle_Elf(_input, _upperAniController ));
        TransitionList.Add(new StateToIdle_Player(_upperAniController , 1f));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)ELF.UpperStateType.ATTACK_START;
    }

    public override void Exit()
    {

    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
    }
}
