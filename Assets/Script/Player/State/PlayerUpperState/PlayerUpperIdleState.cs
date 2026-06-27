using UnityEngine;

public class PlayerUpperIdleState : EntityState
{
    EntityAnimator _upperAniController;
    IEntityInputState _inputState;
    public PlayerUpperIdleState(Player player)
    {
        _upperAniController = player._upperAniController;
        _inputState = player._input;
    }

    public override void Create()
    {
        TransitionList.Add(new IdleToAttackStart_Elf(_inputState));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)ELF.UpperStateType.IDLE;
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
    }
}
