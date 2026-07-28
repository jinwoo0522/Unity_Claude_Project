using UnityEngine;

// 빙결 상태 — 애니메이션을 모든 피어에서 멈춰 그 자세 그대로 굳은 것처럼 표현한다
// 회전은 Rotate를 부르지 않는 것만으로 멈춘다 (상태가 회전 시점을 소유)
// 상체는 현재 포즈를 유지해야 하므로 EMPTY로 보내지 않고 상태머신 자체를 잠근다
// 잠글 대상은 반드시 '부속' 상태머신이어야 한다 — 자신이 속한 상태머신을 넘기면
// 해제 트랜지션까지 멈춰 영구 빙결이 된다 (상체가 없는 몬스터는 넘기지 않는다)
public class EntityFrozenState : EntityState
{
    private Entity _entity;
    private IEntityMovement _move;
    private StateMachine _upperStateMachine;
    private CrowdController _crowdController;
    private IDamagable _damagable;

    public EntityFrozenState(Entity entity, StateMachine upperStateMachine = null)
    {
        _entity = entity;
        _move = entity._move;
        _upperStateMachine = upperStateMachine;
        _crowdController = entity._crowdController;
        _damagable = entity._stat;
    }

    public override void Create()
    {
        TransitionList.Add(new FrozenToIdle_Entity(_crowdController));
    }

    public override void Enter()
    {
        _entity.Set_AnimSpeed(0f);   // 현재 재생 중인 클립을 그 프레임에서 정지

        if(_upperStateMachine == null) return;
        _upperStateMachine.Lock();   // 빙결 중 상체 입력·전환 차단
    }

    public override void Exit()
    {
        // 빙결 중 밀린 피격 플래그를 정리 — 그대로 두면 Unlock 직후 HIT가 뒤늦게 재생된다
        _damagable._isHit = false;

        _entity.Set_AnimSpeed(1f);

        if(_upperStateMachine == null) return;
        _upperStateMachine.Unlock();
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        // 에어본으로 떠 있을 수 있으므로 낙하는 계속 처리한다
        _move.Gravity();
    }
}
