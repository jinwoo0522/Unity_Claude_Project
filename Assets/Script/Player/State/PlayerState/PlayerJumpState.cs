using Unity.Netcode.Components;
using UnityEngine;

public class PlayerJumpState : EntityState
{
    IEntityMovement   _playerMove;
    IJumpMovement     _jump;
    IEntityMoveInput _moveInput;
    EntityAnimator   _aniController;
    public PlayerJumpState(Player player)
    {
        _playerMove = player._move;
        _jump = player._jump;
        _moveInput = player._input;
        _aniController = player._aniController;
    }
    public override void Create()
    {
        TransitionList.Add(new JumpToLand_Player(_jump)); 
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.JUMP;
        _aniController._animator.applyRootMotion = true;
        _jump.Jumping();
    }

    public override void Exit()
    {
        _aniController._animator.applyRootMotion = false;
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _playerMove.Move(_moveInput.MoveInput , _moveInput.isSprint);
        _playerMove.Gravity();
    }
}
