using UnityEngine;

public class PlayerLandState : EntityState
{

    EntityAnimator _aniController;

    PlayerMovement _playerMove;

    public PlayerLandState(Player player)
    {
        _aniController = player._aniController;
        _playerMove = player._move;
    }

    public override void Create()
    {
        TransitionList.Add(new LandToIdle_Player());
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.LAND;    
    }

    public override void Exit()
    {
        
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _playerMove.Gravity();
    }
}
