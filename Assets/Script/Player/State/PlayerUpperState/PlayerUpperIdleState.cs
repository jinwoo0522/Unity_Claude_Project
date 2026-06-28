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
        // Elf 전용 전환(IdleToAttackStart_Elf)은 Elf_Player에서 외부 주입
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)ENTITY.UpperStateType.IDLE;
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
    }
}
