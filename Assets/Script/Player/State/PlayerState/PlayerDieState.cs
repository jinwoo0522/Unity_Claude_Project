using UnityEngine;

// 사망 상태 — 빠져나가는 전환이 없는 종료 상태
// 상체는 완전히 잠그고, 하체는 전환만 잠근다 — 공중에서 죽어도 낙하는 이어져야 한다
public class PlayerDieState : EntityState
{
    private EntityAnimator _aniController;
    private IEntityMovement _move;
    private StateMachine _stateMachine;
    private StateMachine _upperStateMachine;
    private CrowdController _crowdController;

    public PlayerDieState(Player player)
    {
        _aniController = player._aniController;
        _move = player._move;
        _stateMachine = player._stateMachine;
        _upperStateMachine = player._upperStateMachine;
        _crowdController = player._crowdController;
    }

    public override void Create()
    {
        // 사망은 종료 상태 — 빠져나가는 전환이 없다
    }

    public override void Enter()
    {
        // 남아 있던 CC를 모두 해제한다 — 빙결 머티리얼·에어본 이동이 시체에 남지 않게
        _crowdController.RestoreAll();

        _aniController._state.Value = (ushort)ENTITY.StateType.DIE;

        _upperStateMachine.Lock();
        _stateMachine.LockTransition();   // 입력·CC로 사망 상태를 빠져나가는 것만 막는다
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _move.Gravity();   // 공중에서 사망한 경우 그대로 떨어지도록
    }
}
