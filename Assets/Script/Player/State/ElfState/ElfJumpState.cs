using Unity.Netcode.Components;
using UnityEngine;

public class ElfJumpState : EntityState
{
    PlayerMovement _playerMove;
    IEntityMoveInput _moveInput;
    EntityAnimator _aniController;
    public ElfJumpState(Player player)
    {
        _playerMove = player._move;
        _moveInput = player._input;
        _aniController = player._aniController;
    }
    public override void Create()
    {
        TransitionList.Add(new JumpToLand_Player(_playerMove)); 
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.JUMP;
        _playerMove.Jumping();   // 진입 시 1회만 점프
    }

    public override void Exit()
    {
        
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _playerMove.PlayerMove(_moveInput.MoveInput , _moveInput.isSprint);
        _playerMove.Gravity();
    }
}
