using UnityEngine;

public class PlayerIdleState : EntityState
{


    IEntityInputState _inputState;
    IEntityMovement _playerMove;
    EntityAnimator _aniController;
    public PlayerIdleState(Player player)
    {
        _aniController = player._aniController;
        _inputState = player._input;
        _playerMove = player._move;
    }
    public override void Create()
    {
        TransitionList.Add(new IdleToWalk_Player(_inputState)); 
        TransitionList.Add(new StateToMouseAttack_Elf(_inputState));
        TransitionList.Add(new StateToQSkill_Elf(_inputState));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.IDLE;
    }

    public override void Exit()
    {
        
    }

    protected override void UpdateState(float fTimedelta , ushort curState)
    {
        _playerMove.Gravity();
    }
}
