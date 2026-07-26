using UnityEngine;

// 빙결 CC가 풀리면 Idle로 복귀 — 애니메이션 속도 복구는 FrozenState의 Exit가 담당
public class FrozenToIdle_Entity : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.IDLE;


    CrowdController _crowdController;
    public FrozenToIdle_Entity(CrowdController crowdController)
    {
        _crowdController = crowdController;
    }

    public bool CheckRule(float fTimeDelta)
    {
        if(_crowdController.IsApply(CrowdController.CC_TAG.FREEZE) == false)
            return true;

        return false;
    }

    public void OnTransition()
    {
    }
}
