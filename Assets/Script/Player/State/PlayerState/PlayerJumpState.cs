using Unity.Netcode.Components;
using UnityEngine;

public class PlayerJumpState : EntityState
{
    IEntityMovement   _playerMove;
    IJumpMovement     _jump;
    IEntityMoveInput _moveInput;
    IEntityRotate    _rotate;
    EntityAnimator   _aniController;

    float fJumpDelay = 1f;
    public PlayerJumpState(Player player , float fDelay = 1f)
    {
        _playerMove = player._move;
        _rotate = player._rotate;
        _jump = player._jump;
        _moveInput = player._input;
        _aniController = player._aniController;
        fJumpDelay = fDelay;
    }
    public override void Create()
    {
        TransitionList.Add(new JumpToLand_Player(_jump, fJumpDelay)); 
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.JUMP;
        _jump.Jumping();
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _playerMove.Move(_moveInput.MoveInput , _moveInput.isSprint);
        _playerMove.Gravity();
        _rotate.Rotate();
    }
}
