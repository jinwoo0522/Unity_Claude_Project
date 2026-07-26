using UnityEngine;

// 하체가 전신 스킬 동작 중일 때 상체 레이어를 꺼두는 빈 상태
// 진입/복귀는 하체 스킬 상태의 Enter/Exit가 담당한다 (상체 자체 전환 없음)
public class PlayerUpperEmptyState : EntityState
{
    private EntityAnimator _upperAniController;

    public PlayerUpperEmptyState(Player player)
    {
        _upperAniController = player._upperAniController;
    }

    public override void Create()
    {
    }

    public override void Enter()
    {
        // NONE(0)을 넣으면 EntityAnimator가 상체 레이어 weight를 0으로 내린다
        _upperAniController._state.Value = (ushort)ENTITY.UpperStateType.NONE;
    }

    public override void Exit()
    {
    }

    protected override void UpdateState(float fTimedelta, ushort curState)
    {
    }
}
