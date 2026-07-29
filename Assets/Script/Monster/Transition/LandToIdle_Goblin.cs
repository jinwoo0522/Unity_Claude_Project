using UnityEngine;

// 착지 모션이 끝까지 재생되면 대기로 복귀
public class LandToIdle_Goblin : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.IDLE;

    private EntityAnimator _aniController;

    public LandToIdle_Goblin(EntityAnimator aniController)
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
