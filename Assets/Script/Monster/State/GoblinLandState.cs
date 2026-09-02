using UnityEngine;

// 에어본이 끝난 뒤 착지 경직 — 착지 모션이 끝날 때까지 이동을 막는다 (회전은 유지)
public class GoblinLandState : EntityState
{
    private IEntityMovement _move;
    private EntityAnimator _aniController;

    public GoblinLandState(Goblin goblin)
    {
        _move = goblin._move;
        _aniController = goblin._aniController;
    }

    public override void Create()
    {
        TransitionList.Add(new LandToIdle_Goblin(_aniController));
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
        // 착지 중에는 이동만 막고 회전은 유지한다
        _move.Gravity();
    }
}
