using UnityEngine;

// 빙결 상태 — 애니메이션을 모든 피어에서 멈춰 그 자세 그대로 굳은 것처럼 표현한다
// 상체는 현재 포즈를 유지해야 하므로 EMPTY로 보내지 않고 상태머신 자체를 잠근다
public class PlayerFrozenState : EntityState
{
    Player _player;
    IEntityMovement _move;
    StateMachine _upperStateMachine;
    CrowdController _crowdController;
    IDamagable _damagable;
    PlayerCameraRotate _camRotater;

    public PlayerFrozenState(Player player)
    {
        _player = player;
        _move = player._move;
        _upperStateMachine = player._upperStateMachine;
        _crowdController = player._crowdController;
        _damagable = player._stat;
        _camRotater = player._camRotater;
    }

    public override void Create()
    {
        TransitionList.Add(new FrozenToIdle_Entity(_crowdController));
    }

    public override void Enter()
    {
        _player.Set_AnimSpeed(0f);   // 현재 재생 중인 클립을 그 프레임에서 정지
        _upperStateMachine.Lock();   // 빙결 중 상체 입력·전환 차단
        _camRotater.Lock();          // 굳은 자세 유지 — 조준을 돌려도 몸통이 따라 돌지 않게 한다
    }

    public override void Exit()
    {
        // 빙결 중 밀린 피격 플래그를 정리 — 그대로 두면 Unlock 직후 HIT가 뒤늦게 재생된다
        _damagable._isHit = false;

        _player.Set_AnimSpeed(1f);
        _upperStateMachine.Unlock();
        _camRotater.Unlock();
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        // 에어본으로 떠 있을 수 있으므로 낙하는 계속 처리한다
        _move.Gravity();
    }
}
