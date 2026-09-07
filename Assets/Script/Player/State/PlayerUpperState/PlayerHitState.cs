using UnityEngine;

public class PlayerHitState : EntityState
{
    EntityAnimator _upperAniController;
    IDamagable _damagable;

    public PlayerHitState(Player player)
    {
        _upperAniController = player._upperAniController;
        _damagable = player._damagable;
    }
    public override void Create()
    {
        // Hit To None
        TransitionList.Add(new StateToIdle_Player(_upperAniController));
    }

    public override void Enter()
    {
        Debug.Log($"[HitState] Enter frame={Time.frameCount}");
        _upperAniController._state.Value = (ushort)ENTITY.UpperStateType.HIT;
    }

    public override void Exit()
    {
        Debug.Log("히트 끝");
        _damagable._isHit = false;
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
    }
}
