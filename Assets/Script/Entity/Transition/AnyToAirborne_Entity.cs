using UnityEngine;

public class AnyToAirborne_Entity : ITransition
{
    public ushort NextState => (ushort)ENTITY.StateType.AIRBORNE;


    CrowdController _crowdController;
    public AnyToAirborne_Entity(CrowdController crowdController)
    {
        _crowdController = crowdController;
    }

    public bool CheckRule(float fTimeDelta)
    {
        // 빙결 중에는 에어본 상태로 넘어가지 않는다 (몸을 띄우는 건 CC_AirBorne이 계속 수행)
        if(_crowdController.IsApply(CrowdController.CC_TAG.FREEZE) == true)
            return false;

        if(_crowdController.IsApply(CrowdController.CC_TAG.AIRBORNE) == true)
            return true;

        return false;
    }

    public void OnTransition()
    {
    }
}
