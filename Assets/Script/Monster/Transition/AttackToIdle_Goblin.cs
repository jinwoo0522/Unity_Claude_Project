using UnityEngine;

// 공격 모션이 끝까지 재생되면 대기로 복귀 — 다시 붙일지는 대기가 판단한다
public class AttackToIdle_Goblin : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.IDLE;

    private EntityAnimator _aniController;

    public AttackToIdle_Goblin(EntityAnimator aniController)
    {
        _aniController = aniController;
    }

    public bool CheckRule(float fTimeDelta)
    {
        return _aniController.IsCurrentStateFinished();
    }

    public void OnTransition()
    {
    }
}
