using UnityEngine;

// 대기 중에는 타게터로 주변 플레이어만 탐색한다 — 타겟이 잡히면 전환이 MOVE로 넘긴다
public class GoblinIdleState : EntityState
{
    private MonsterTargeter _targeter;
    private IEntityMovement _move;
    private EntityAnimator _aniController;

    private float _fDetectRange;

    public GoblinIdleState(Goblin goblin)
    {
        _targeter = goblin._targeter;
        _move = goblin._move;
        _aniController = goblin._aniController;
        _fDetectRange = goblin._stat._data.fDetectRange;
    }

    public override void Create()
    {
        TransitionList.Add(new IdleToMove_Goblin(_targeter));
    }

    public override void Enter()
    {
        _aniController._state.Value = (ushort)ENTITY.StateType.IDLE;
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
        _targeter.Search(_fDetectRange);
        _move.Gravity();
    }
}
