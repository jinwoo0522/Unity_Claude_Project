using UnityEngine;

public class PlayerLandState : EntityState
{

    EntityAnimator _aniController;

    IEntityMovement _playerMove;

    IEntityRotate _rotate;

    public PlayerLandState(Player player)
    {
        _aniController = player._aniController;
        _playerMove = player._move;
        _rotate = player._rotate;
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
        _rotate.Rotate();
    }
}
