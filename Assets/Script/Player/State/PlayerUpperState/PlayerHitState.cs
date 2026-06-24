using UnityEngine;

public class PlayerHitState : EntityState
{
    EntityAnimator _upperAniController;

    public PlayerHitState(Player player)
    {
        _upperAniController = player._upperAniController;
    }
    public override void Create()
    {
        // Hit To None
        TransitionList.Add(new StateToIdle_Player(_upperAniController));
    }

    public override void Enter()
    {
        _upperAniController._state.Value = (ushort)ENTITY.UpperStateType.HIT;
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
    }
}
