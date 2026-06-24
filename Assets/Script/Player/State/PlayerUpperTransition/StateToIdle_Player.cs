using UnityEngine;

public class StateToIdle_Player : ITransition
{
    EntityAnimator _upperAniController;

    float          fTransitionDelay;
    float          fAccTime;

    public StateToIdle_Player(EntityAnimator upperAniController, float fDelay = 0f)
    {
        _upperAniController = upperAniController;
        fTransitionDelay = fDelay;
    }
    public ushort NextState => (ushort)ENTITY.UpperStateType.IDLE;
    public bool CheckRule(float fTimeDelta)
    {
        // 내 HIT 애니메이션이 끝까지 재생되면 전환
        if(_upperAniController.IsCurrentStateFinished() == false)
            return false;

        fAccTime += fTimeDelta;

        if(fTransitionDelay > fAccTime)
            return false;   

        return true;
    }

    public void OnTransition()
    {
        fAccTime = 0f;
        _upperAniController._state.Value = (ushort)ENTITY.UpperStateType.IDLE;
        Debug.Log("None Transition");
    }
}
