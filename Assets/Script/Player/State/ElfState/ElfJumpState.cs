using Unity.Netcode.Components;
using UnityEngine;

public class ElfJumpState : EntityState
{
    PlayerMovement   _playerMove;
    IEntityMoveInput _moveInput;
    EntityAnimator   _aniController;

    float fJumpDelay;
    float fCurTime;
    bool isOnce;


    public ElfJumpState(Player player , float fDelay = 0.25f)
    {
        _playerMove = player._move;
        _moveInput = player._input;
        _aniController = player._aniController;
        fJumpDelay = fDelay;
    }
    public override void Create()
    {
        TransitionList.Add(new JumpToLand_Player(_playerMove)); 
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.JUMP;
        
        fCurTime = Time.time;
        isOnce = false;
    }

    public override void Exit()
    {
        
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {

        if(fCurTime + fJumpDelay < Time.time && isOnce == false) // 점프 딜레이 주기
        {
            _playerMove.Jumping();
            isOnce = true;          
        }
        else
        {
            _playerMove.PlayerMove(_moveInput.MoveInput , _moveInput.isSprint);
        }

        _playerMove.Gravity();


    }
}
