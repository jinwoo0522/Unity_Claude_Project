using UnityEngine;

// 타게터가 잡아둔 대상(추격 타겟 또는 복귀 지점)을 향해 이동한다
// 이탈·타겟 사망 판정은 타게터가 담당하고, 상태는 호출 시점만 소유한다
// 상태 태그는 걷기/달리기 구분이 없는 몬스터라 WALK 하나만 쓴다
public class GoblinMoveState : EntityState
{
    private MonsterTargeter _targeter;
    private IEntityMovement _move;
    private EntityAnimator _aniController;

    private float _fLeaveRange;
    private float _fStopDistance;

    public GoblinMoveState(Goblin goblin)
    {
        _targeter = goblin._targeter;
        _move = goblin._move;
        _aniController = goblin._aniController;
        _fLeaveRange = goblin._enemyData.fReturnDistance;
        // 공격 진입 거리는 NavMesh가 멈추는 지점과 같아야 하므로 에이전트 값을 그대로 쓴다
        _fStopDistance = goblin._agent.stoppingDistance;
    }

    public override void Create()
    {
        // 등록 순서가 곧 우선순위 — 타겟을 놓친 경우를 먼저 걸러야 공격 판정이 빈 타겟을 보지 않는다
        TransitionList.Add(new MoveToIdle_Goblin(_targeter));
        TransitionList.Add(new MoveToAttack_Goblin(_targeter, _fStopDistance));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.WALK;
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        // 추격 중에는 재탐색 없이 잡아둔 타겟만 붙든다 — 놓치면 전환이 대기로 넘긴다
        _targeter.KeepTarget(_fLeaveRange);
        // 이동 방향은 타게터가 정하므로 입력 인자는 비워 보낸다
        _move.Move(Vector2.zero, false);
        _move.Gravity();
    }
}
