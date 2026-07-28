using UnityEngine;

public class EntityAirborneState : EntityState
{
    EntityAnimator _aniController;
    IEntityMovement _move;
    IEntityRotate _rotate;
    CrowdController _crowdController;
    public EntityAirborneState(Entity entity)
    {
        _aniController = entity._aniController;
        _move = entity._move;
        _rotate = entity._rotate;
        _crowdController = entity._crowdController;
    }
    public override void Create()
    {
        TransitionList.Add(new AirborneToLand_Entity(_crowdController)); 
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.AIRBORNE;
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _move.Gravity();
        _rotate.Rotate();
    }
}
