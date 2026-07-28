using UnityEngine;

// 타게터가 잡아둔 대상(추격 타겟 또는 복귀 지점)을 향해 이동한다
// 이탈·타겟 사망 판정은 타게터가 담당하고, 상태는 호출 시점만 소유한다
// 상태 태그는 걷기/달리기 구분이 없는 몬스터라 WALK 하나만 쓴다
public class GoblinMoveState : EntityState
{
    private MonsterTargeter _targeter;
    private IEntityMovement _move;
    private IEntityRotate _rotate;
    private EntityAnimator _aniController;

    private float _fReturnDistance;

    public GoblinMoveState(Goblin goblin)
    {
        _targeter = goblin._targeter;
        _move = goblin._move;
        _rotate = goblin._rotate;
        _aniController = goblin._aniController;
        _fReturnDistance = goblin._stat._data.fReturnDistance;
    }

    public override void Create()
    {
        TransitionList.Add(new MoveToIdle_Goblin(_targeter));
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
        _targeter.ReturnPoint(_fReturnDistance);

        // 이동 방향은 타게터가 정하므로 입력 인자는 비워 보낸다
        _move.Move(Vector2.zero, false);
        _move.Gravity();
        _rotate.Rotate();
    }
}
