using Unity.Netcode.Components;
using UnityEngine;

public class ElfJumpState : EntityState
{
    IJumpMovement   _jump;
    IEntityMovement _move;
    IEntityMoveInput _moveInput;
    IEntityRotate    _rotate;
    EntityAnimator   _aniController;

    float fJumpDelay;
    float fCurTime;
    bool isOnce;


    public ElfJumpState(Player player , float fDelay = 0.25f)
    {
        _jump = player._jump;
        _move = player._move;
        _rotate = player._rotate;
        _moveInput = player._input;
        _aniController = player._aniController;
        fJumpDelay = fDelay;
    }
    public override void Create()
    {
        TransitionList.Add(new JumpToLand_Player(_jump)); 
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
            _jump.Jumping();
            isOnce = true;          
        }
        else
        {
            _move.Move(_moveInput.MoveInput , _moveInput.isSprint);
        }

        _move.Gravity();
        _rotate.Rotate();
    }
}
